namespace TheStarRichyApi.Models.Kbank
{
    public class KbankSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ConsumerKey { get; set; } = string.Empty;
        public string ConsumerSecret { get; set; } = string.Empty;
        public string PartnerId { get; set; } = string.Empty;
        public string PartnerSecret { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;
        public string TerminalId { get; set; } = string.Empty;
    }
}
