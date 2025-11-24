using System.Text.Json.Serialization;

namespace TheStarRichyApi.Models.Kbank
{
    /// <summary>
    /// KBank Webhook Callback Request (Updated with txnStatus)
    /// Endpoint: POST https://yourdomain.com/qr/payment-callback
    /// </summary>
    public class KbankWebhookRequest
    {
        [JsonPropertyName("partnerTxnUid")]
        public string PartnerTxnUid { get; set; } = string.Empty;

        [JsonPropertyName("partnerId")]
        public string PartnerId { get; set; } = string.Empty;

        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>
        /// Transaction Status from KBank
        /// Values: PAID, CANCELLED, EXPIRED, REQUESTED, VOIDED
        /// </summary>
        [JsonPropertyName("txnStatus")]
        public string? TxnStatus { get; set; }

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("errorDesc")]
        public string? ErrorDesc { get; set; }

        [JsonPropertyName("merchantId")]
        public string MerchantId { get; set; } = string.Empty;

        [JsonPropertyName("txnAmount")]
        public decimal TxnAmount { get; set; }

        [JsonPropertyName("txnCurrencyCode")]
        public string TxnCurrencyCode { get; set; } = "THB";

        [JsonPropertyName("loyaltyId")]
        public string? LoyaltyId { get; set; }

        [JsonPropertyName("txnNo")]
        public string? TxnNo { get; set; }

        [JsonPropertyName("additionalInfo")]
        public string? AdditionalInfo { get; set; }

        [JsonPropertyName("cardScheme")]
        public string? CardScheme { get; set; }

        [JsonPropertyName("cardNo")]
        public string? CardNo { get; set; }

        [JsonPropertyName("approvalCode")]
        public string? ApprovalCode { get; set; }

        [JsonPropertyName("channel")]
        public string? Channel { get; set; }

        [JsonPropertyName("terminalId")]
        public string TerminalId { get; set; } = string.Empty;

        [JsonPropertyName("qrType")]
        public string? QrType { get; set; }

        [JsonPropertyName("reference1")]
        public string? Reference1 { get; set; }

        [JsonPropertyName("reference2")]
        public string? Reference2 { get; set; }

        [JsonPropertyName("reference3")]
        public string? Reference3 { get; set; }

        [JsonPropertyName("reference4")]
        public string? Reference4 { get; set; }
    }

    /// <summary>
    /// KBank Webhook Callback Response
    /// </summary>
    public class KbankWebhookResponse
    {
        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; } = "00";

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("errorDesc")]
        public string? ErrorDesc { get; set; }
    }

    /// <summary>
    /// Internal Webhook Processing Result
    /// </summary>
    public class WebhookProcessResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string OrderID { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? TransactionStatus { get; set; }
        public decimal Amount { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorDesc { get; set; }
    }

    /// <summary>
    /// KBank Transaction Status Constants
    /// </summary>
    public static class KbankTransactionStatus
    {
        /// <summary>
        /// Transaction has been paid.
        /// </summary>
        public const string Paid = "PAID";

        /// <summary>
        /// QR is cancelled and cannot be used.
        /// For QR Credit Card: Only log as CANCELLED but QR is still active.
        /// </summary>
        public const string Cancelled = "CANCELLED";

        /// <summary>
        /// QR is expired and cannot be used.
        /// </summary>
        public const string Expired = "EXPIRED";

        /// <summary>
        /// QR is requested but not yet paid or cancelled.
        /// </summary>
        public const string Requested = "REQUESTED";

        /// <summary>
        /// Transaction is voided after it is paid.
        /// </summary>
        public const string Voided = "VOIDED";
    }

    /// <summary>
    /// Payment Status (internal)
    /// </summary>
    public static class PaymentStatus
    {
        public const string Paid = "Paid";
        public const string Cancelled = "Cancelled";
        public const string Expired = "Expired";
        public const string Pending = "Pending";
        public const string Voided = "Voided";
        public const string Failed = "Failed";
    }

    /// <summary>
    /// Status Code Constants
    /// </summary>
    public static class KbankStatusCode
    {
        public const string Success = "00";
        public const string Pending = "01";
        public const string Failed = "02";
        public const string Cancelled = "03";
        public const string Expired = "04";
        public const string Refunded = "05";
    }
}