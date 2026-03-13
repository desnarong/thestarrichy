namespace TheStarRichyApi.Models
{
    /// <summary>
    /// DTO สำหรับที่อยู่สมาชิก 1 รายการ (ใช้ทั้ง GET และ PUT)
    /// AddressType: 1=ตามบัตร  2=ปัจจุบัน  3=ออกใบกำกับภาษี
    /// </summary>
    public class MemberAddressDto
    {
        public int? Id { get; set; }

        /// <summary>1=ตามบัตร  2=ปัจจุบัน  3=ออกใบกำกับภาษี</summary>
        public int AddressType { get; set; }

        // =========== ที่อยู่แบบละเอียด ===========
        public string? HouseNumber { get; set; }   // บ้านเลขที่
        public string? Moo         { get; set; }   // หมู่ที่
        public string? Alley       { get; set; }   // ซอย
        public string? Road        { get; set; }   // ถนน
        public string? Building    { get; set; }   // อาคาร / หมู่บ้าน
        public string? Floor       { get; set; }   // ชั้น
        public string? Other       { get; set; }   // อื่นๆ

        // =========== ตำแหน่งที่ตั้ง ===========
        public int?    TambonId    { get; set; }   // FK → TBL_TAMBONS.id
        public string? Zipcode     { get; set; }   // รหัสไปรษณีย์

        // =========== Type 3: ออกใบกำกับภาษี ===========
        public string? CompanyName  { get; set; }  // ชื่อบริษัท / ชื่อผู้รับใบกำกับ
        public string? CompanyTaxId { get; set; }  // เลขประจำตัวผู้เสียภาษี
        public string? BranchCode   { get; set; }  // รหัสสาขา (00000 = สำนักงานใหญ่)
        public string? BranchName   { get; set; }  // ชื่อสาขา

        // =========== Read-only: จาก JOIN TBL_TAMBONS ===========
        public string? Tambon       { get; set; }
        public string? Amphoe       { get; set; }
        public string? Province     { get; set; }
        public string? TambonCode   { get; set; }
        public string? AmphoeCode   { get; set; }
        public string? ProvinceCode { get; set; }
    }

    /// <summary>
    /// Request body สำหรับ PUT /Member/profile-addresses
    /// ส่งมาเป็น array ของที่อยู่ทุกประเภทที่ต้องการบันทึก
    /// </summary>
    public class UpsertMemberAddressesRequest
    {
        public List<MemberAddressDto> Addresses { get; set; } = new();
    }
}
