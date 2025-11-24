using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using TheStarRichyApi.Models.Kbank;

namespace TheStarRichyApi.Services
{
    public interface IKbankWebhookService
    {
        Task<WebhookProcessResult> ProcessWebhookAsync(KbankWebhookRequest request);
        Task<WebhookProcessResult> CallUpdatePaymentStatusAsync(string orderID);
    }

    public class KbankWebhookService : IKbankWebhookService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KbankWebhookService> _logger;
        private readonly string _connectionString;

        public KbankWebhookService(
            IConfiguration configuration,
            ILogger<KbankWebhookService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("MLMConnectionString");
        }

        /// <summary>
        /// Process KBank Webhook Callback
        /// ✅ รองรับ txnStatus: PAID, CANCELLED, EXPIRED, REQUESTED, VOIDED
        /// </summary>
        public async Task<WebhookProcessResult> ProcessWebhookAsync(KbankWebhookRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Processing webhook - PartnerTxnUid: {PartnerTxnUid}, StatusCode: {StatusCode}, TxnStatus: {TxnStatus}, Amount: {Amount}, TxnNo: {TxnNo}",
                    request.PartnerTxnUid, request.StatusCode, request.TxnStatus, request.TxnAmount, request.TxnNo);

                // 1. Get OrderID from Reference1
                string orderID = request.Reference1 ?? "";
                if (string.IsNullOrEmpty(orderID))
                {
                    _logger.LogWarning("No OrderID in Reference1 for webhook: {PartnerTxnUid}", request.PartnerTxnUid);
                    return new WebhookProcessResult
                    {
                        Success = false,
                        Message = "No OrderID in Reference1",
                        ErrorCode = "E001",
                        ErrorDesc = "Missing OrderID"
                    };
                }

                // 2. ✅ Determine Payment Status from txnStatus (preferred) or statusCode
                string paymentStatus = DeterminePaymentStatus(request.TxnStatus, request.StatusCode);
                string transactionStatus = request.TxnStatus ?? "";

                _logger.LogInformation(
                    "Mapped status - TxnStatus: {TxnStatus}, StatusCode: {StatusCode} → PaymentStatus: {PaymentStatus}",
                    request.TxnStatus, request.StatusCode, paymentStatus);

                // 3. Prepare Webhook Data (full JSON)
                string webhookData = JsonSerializer.Serialize(request);

                // 4. ✅ Call SP_UpdatePaymentFromWebhook
                var result = await CallUpdatePaymentFromWebhookAsync(
                    orderID,
                    request.PartnerTxnUid,
                    paymentStatus,
                    transactionStatus,
                    request.TxnNo,
                    request.TxnAmount,
                    request.ApprovalCode,
                    request.Channel,
                    request.CardScheme,
                    request.CardNo,
                    request.StatusCode,
                    request.ErrorCode,
                    request.ErrorDesc,
                    webhookData
                );

                if (result.Success)
                {
                    _logger.LogInformation(
                        "Successfully updated order {OrderID} - PaymentStatus: {PaymentStatus}, TxnStatus: {TxnStatus}",
                        orderID, paymentStatus, transactionStatus);
                }
                else
                {
                    _logger.LogError("Failed to update order {OrderID}: {Message}", orderID, result.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook: {PartnerTxnUid}", request.PartnerTxnUid);
                return new WebhookProcessResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    ErrorCode = "E999",
                    ErrorDesc = "Internal error"
                };
            }
        }

        /// <summary>
        /// ✅ Call SP_UpdatePaymentFromWebhook (with txnStatus)
        /// </summary>
        private async Task<WebhookProcessResult> CallUpdatePaymentFromWebhookAsync(
            string orderID,
            string partnerTxnUid,
            string paymentStatus,
            string transactionStatus,
            string txnNo,
            decimal amount,
            string approvalCode,
            string channel,
            string cardScheme,
            string cardNo,
            string statusCode,
            string errorCode,
            string errorDesc,
            string webhookData)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("SP_UpdatePaymentFromWebhook", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 30;

                        // Input Parameters
                        command.Parameters.AddWithValue("@OrderID", orderID);
                        command.Parameters.AddWithValue("@PartnerTxnUid", partnerTxnUid);
                        command.Parameters.AddWithValue("@PaymentStatus", paymentStatus);
                        command.Parameters.AddWithValue("@TransactionStatus", transactionStatus ?? (object)DBNull.Value); // ✅ NEW
                        command.Parameters.AddWithValue("@TxnNo", txnNo ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Amount", amount);
                        command.Parameters.AddWithValue("@ApprovalCode", approvalCode ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Channel", channel ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CardScheme", cardScheme ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CardNo", cardNo ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@StatusCode", statusCode ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ErrorCode", errorCode ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ErrorDesc", errorDesc ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@WebhookData", webhookData);

                        // Execute and read result
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                bool success = reader.GetInt32(reader.GetOrdinal("Success")) == 1;
                                string message = reader.GetString(reader.GetOrdinal("Message"));

                                var result = new WebhookProcessResult
                                {
                                    Success = success,
                                    Message = message,
                                    OrderID = orderID,
                                    PaymentStatus = paymentStatus,
                                    TransactionStatus = transactionStatus,
                                    Amount = amount
                                };

                                return result;
                            }
                            else
                            {
                                return new WebhookProcessResult
                                {
                                    Success = false,
                                    Message = "No result from stored procedure",
                                    ErrorCode = "E002",
                                    ErrorDesc = "SP execution failed"
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error calling SP_UpdatePaymentFromWebhook: {OrderID}", orderID);
                return new WebhookProcessResult
                {
                    Success = false,
                    Message = $"Database error: {sqlEx.Message}",
                    ErrorCode = "E003",
                    ErrorDesc = "SQL Error"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling SP_UpdatePaymentFromWebhook: {OrderID}", orderID);
                return new WebhookProcessResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    ErrorCode = "E999",
                    ErrorDesc = "Internal error"
                };
            }
        }

        public async Task<WebhookProcessResult> CallUpdatePaymentStatusAsync(string orderID)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("SP_UpdatePaymentStatus", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 30;

                        // Input Parameters
                        command.Parameters.AddWithValue("@OrderID", orderID);
                        command.Parameters.AddWithValue("@PaymentStatus", "Success");

                        // Execute and read result
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                bool success = reader.GetInt32(reader.GetOrdinal("Success")) == 1;
                                string message = reader.GetString(reader.GetOrdinal("Message"));

                                var result = new WebhookProcessResult
                                {
                                    Success = success,
                                    Message = message,
                                    OrderID = orderID
                                };

                                return result;
                            }
                            else
                            {
                                return new WebhookProcessResult
                                {
                                    Success = false,
                                    Message = "No result from stored procedure",
                                    ErrorCode = "E002",
                                    ErrorDesc = "SP execution failed"
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error calling SP_UpdatePaymentFromWebhook: {OrderID}", orderID);
                return new WebhookProcessResult
                {
                    Success = false,
                    Message = $"Database error: {sqlEx.Message}",
                    ErrorCode = "E003",
                    ErrorDesc = "SQL Error"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling SP_UpdatePaymentFromWebhook: {OrderID}", orderID);
                return new WebhookProcessResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    ErrorCode = "E999",
                    ErrorDesc = "Internal error"
                };
            }
        }

        /// <summary>
        /// ✅ Determine Payment Status from txnStatus (preferred) or statusCode
        /// </summary>
        private string DeterminePaymentStatus(string txnStatus, string statusCode)
        {
            // ✅ Priority 1: Check txnStatus (from KBank)
            if (!string.IsNullOrEmpty(txnStatus))
            {
                return txnStatus.ToUpper() switch
                {
                    KbankTransactionStatus.Paid => PaymentStatus.Paid,           // PAID
                    KbankTransactionStatus.Cancelled => PaymentStatus.Cancelled,  // CANCELLED
                    KbankTransactionStatus.Expired => PaymentStatus.Expired,      // EXPIRED
                    KbankTransactionStatus.Requested => PaymentStatus.Pending,    // REQUESTED
                    KbankTransactionStatus.Voided => PaymentStatus.Voided,        // VOIDED
                    _ => PaymentStatus.Pending
                };
            }

            // ✅ Priority 2: Fallback to statusCode
            return statusCode switch
            {
                "00" => PaymentStatus.Paid,           // Success
                "01" => PaymentStatus.Pending,        // Pending
                "02" => PaymentStatus.Failed,         // Failed
                "03" => PaymentStatus.Cancelled,      // Cancelled
                "04" => PaymentStatus.Expired,        // Expired
                "05" => PaymentStatus.Voided,         // Refunded/Voided
                _ => PaymentStatus.Pending
            };
        }
    }
}