namespace TheStarRichyApi.Models
{
    public class UpdateMemberProfileRequest
    {
        public PersonalInfoSection PersonalInfo { get; set; } = new();
        public AddressInfoSection AddressInfo { get; set; } = new();
        public InvoiceInfoSection InvoiceInfo { get; set; } = new();
        public BankInfoSection BankInfo { get; set; } = new();
        public DocumentInfoSection DocumentInfo { get; set; } = new();
        public string? UpdateScope { get; set; }
    }

    public class PersonalInfoSection
    {
        public string? BussinessName { get; set; }
        public string? LanguageCode { get; set; }
    }

    public class AddressInfoSection
    {
        public AddressBlock? IdCardAddress { get; set; }
        public AddressBlock? PresentAddress { get; set; }
    }

    public class InvoiceInfoSection
    {
        public string? CompanyName { get; set; }
        public string? CompanyRegistrationNo { get; set; }
        public AddressBlock? InvoiceAddress { get; set; }
    }

    public class BankInfoSection
    {
        public string? BankCode { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public string? BranchName { get; set; }
        public string? Beneficiary { get; set; }
        public string? BeneficiaryIdCard { get; set; }
    }

    public class DocumentInfoSection
    {
        public string? ProfileImageUrl { get; set; }
        public string? IdCardImageUrl { get; set; }
        public string? BankBookImageUrl { get; set; }
        public string? ApplicationFormImageUrl { get; set; }
    }

    public class AddressBlock
    {
        public string? AddressLine { get; set; }
        public string? Postcode { get; set; }
        public string? ProvinceCode { get; set; }
        public string? ProvinceName { get; set; }
        public string? DistrictCode { get; set; }
        public string? DistrictName { get; set; }
        public string? SubdistrictCode { get; set; }
        public string? SubdistrictName { get; set; }
    }

    public class UpdateMemberProfileResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class UpdateMemberProfilePic4Request
    {
        public string? ProfileImageUrl { get; set; }
    }
}
