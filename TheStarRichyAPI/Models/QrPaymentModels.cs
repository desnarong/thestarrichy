using System.Text.Json.Serialization;

namespace TheStarRichyApi.Models.Kbank
{
    // QR Payment Request
    public class QrPaymentRequest
    {
        [JsonPropertyName("partnerTxnUid")]
        public string PartnerTxnUid { get; set; } = string.Empty;

        [JsonPropertyName("partnerId")]
        public string PartnerId { get; set; } = string.Empty;

        [JsonPropertyName("partnerSecret")]
        public string PartnerSecret { get; set; } = string.Empty;

        [JsonPropertyName("requestDt")]
        public string RequestDt { get; set; } = string.Empty;

        [JsonPropertyName("merchantId")]
        public string MerchantId { get; set; } = string.Empty;

        [JsonPropertyName("terminalId")]
        public string TerminalId { get; set; } = string.Empty;

        [JsonPropertyName("qrType")]
        public string QrType { get; set; } = "3"; // Default Thai QR

        [JsonPropertyName("txnAmount")]
        public decimal TxnAmount { get; set; }

        [JsonPropertyName("txnCurrencyCode")]
        public string TxnCurrencyCode { get; set; } = "THB";

        [JsonPropertyName("reference1")]
        public string? Reference1 { get; set; }

        [JsonPropertyName("reference2")]
        public string? Reference2 { get; set; }

        [JsonPropertyName("reference3")]
        public string? Reference3 { get; set; }

        [JsonPropertyName("reference4")]
        public string? Reference4 { get; set; }

        [JsonPropertyName("metadata")]
        public string? Metadata { get; set; }
    }

    // QR Payment Response
    public class QrPaymentResponse
    {
        [JsonPropertyName("partnerTxnUid")]
        public string PartnerTxnUid { get; set; } = string.Empty;

        [JsonPropertyName("partnerId")]
        public string PartnerId { get; set; } = string.Empty;

        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; } = string.Empty;

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("errorDesc")]
        public string? ErrorDesc { get; set; }

        [JsonPropertyName("accountName")]
        public string? AccountName { get; set; }

        [JsonPropertyName("qrCode")]
        public string? QrCode { get; set; }

        [JsonPropertyName("sof")]
        public List<string>? Sof { get; set; }
    }

    // Inquiry Request
    public class QrInquiryRequest
    {
        [JsonPropertyName("partnerTxnUid")]
        public string PartnerTxnUid { get; set; } = string.Empty;

        [JsonPropertyName("origPartnerTxnUid")]
        public string OrigPartnerTxnUid { get; set; } = string.Empty;

        [JsonPropertyName("partnerId")]
        public string PartnerId { get; set; } = string.Empty;

        [JsonPropertyName("partnerSecret")]
        public string PartnerSecret { get; set; } = string.Empty;

        [JsonPropertyName("requestDt")]
        public string RequestDt { get; set; } = string.Empty;

        [JsonPropertyName("merchantId")]
        public string MerchantId { get; set; } = string.Empty;

        [JsonPropertyName("terminalId")]
        public string TerminalId { get; set; } = string.Empty;
    }

    // Inquiry Response
    public class QrInquiryResponse
    {
        [JsonPropertyName("partnerTxnUid")]
        public string PartnerTxnUid { get; set; } = string.Empty;

        [JsonPropertyName("partnerId")]
        public string PartnerId { get; set; } = string.Empty;

        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; } = string.Empty;

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("errorDesc")]
        public string? ErrorDesc { get; set; }

        [JsonPropertyName("txnStatus")]
        public string? TxnStatus { get; set; }

        [JsonPropertyName("txnNo")]
        public string? TxnNo { get; set; }

        [JsonPropertyName("loyaltyId")]
        public string? LoyaltyId { get; set; }

        [JsonPropertyName("channel")]
        public string? Channel { get; set; }

        [JsonPropertyName("merchantId")]
        public string? MerchantId { get; set; }

        [JsonPropertyName("terminalId")]
        public string? TerminalId { get; set; }

        [JsonPropertyName("qrType")]
        public string? QrType { get; set; }

        [JsonPropertyName("txnAmount")]
        public string? TxnAmount { get; set; }

        [JsonPropertyName("txnCurrencyCode")]
        public string? TxnCurrencyCode { get; set; }

        [JsonPropertyName("reference1")]
        public string? Reference1 { get; set; }

        [JsonPropertyName("reference2")]
        public string? Reference2 { get; set; }

        [JsonPropertyName("reference3")]
        public string? Reference3 { get; set; }

        [JsonPropertyName("reference4")]
        public string? Reference4 { get; set; }
    }

    // Cancel/Void Request
    public class QrCancelRequest
    {
        [JsonPropertyName("partnerTxnUid")]
        public string PartnerTxnUid { get; set; } = string.Empty;

        [JsonPropertyName("origPartnerTxnUid")]
        public string OrigPartnerTxnUid { get; set; } = string.Empty;

        [JsonPropertyName("partnerId")]
        public string PartnerId { get; set; } = string.Empty;

        [JsonPropertyName("partnerSecret")]
        public string PartnerSecret { get; set; } = string.Empty;

        [JsonPropertyName("requestDt")]
        public string RequestDt { get; set; } = string.Empty;

        [JsonPropertyName("merchantId")]
        public string MerchantId { get; set; } = string.Empty;

        [JsonPropertyName("terminalId")]
        public string TerminalId { get; set; } = string.Empty;
    }

    // Cancel/Void Response
    public class QrCancelResponse
    {
        [JsonPropertyName("partnerTxnUid")]
        public string PartnerTxnUid { get; set; } = string.Empty;

        [JsonPropertyName("partnerId")]
        public string PartnerId { get; set; } = string.Empty;

        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; } = string.Empty;

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("errorDesc")]
        public string? ErrorDesc { get; set; }
    }

    // Settlement Request
    public class QrSettlementRequest
    {
        [JsonPropertyName("partnerTxnUid")]
        public string PartnerTxnUid { get; set; } = string.Empty;

        [JsonPropertyName("partnerId")]
        public string PartnerId { get; set; } = string.Empty;

        [JsonPropertyName("partnerSecret")]
        public string PartnerSecret { get; set; } = string.Empty;

        [JsonPropertyName("requestDt")]
        public string RequestDt { get; set; } = string.Empty;

        [JsonPropertyName("merchantId")]
        public string MerchantId { get; set; } = string.Empty;

        [JsonPropertyName("terminalId")]
        public string TerminalId { get; set; } = string.Empty;

        [JsonPropertyName("qrType")]
        public string QrType { get; set; } = "3";
    }

    // Settlement Response
    public class QrSettlementResponse
    {
        [JsonPropertyName("partnerTxnUid")]
        public string PartnerTxnUid { get; set; } = string.Empty;

        [JsonPropertyName("partnerId")]
        public string PartnerId { get; set; } = string.Empty;

        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; } = string.Empty;

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("errorDesc")]
        public string? ErrorDesc { get; set; }

        [JsonPropertyName("accountNo")]
        public string? AccountNo { get; set; }

        [JsonPropertyName("accountName")]
        public string? AccountName { get; set; }

        [JsonPropertyName("settlementAmount")]
        public decimal? SettlementAmount { get; set; }

        [JsonPropertyName("settlementCurrencyCode")]
        public string? SettlementCurrencyCode { get; set; }
    }
}
