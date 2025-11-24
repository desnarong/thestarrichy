namespace TheStarRichyApi.Models
{
    // Request Models
    public class AddToCartRequest
    {
        public string ProductID { get; set; }
        public string? ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public decimal PV { get; set; }
        public decimal ShippingFee { get; set; }
        public int Quantity { get; set; }
        public string Makerby { get; set; }

        // ⭐ DL และ Center Information
        public string? DLCode { get; set; }                 // รหัสผู้แนะนำ
        public string? DLName { get; set; }                 // ชื่อผู้แนะนำ
        public DateTime? RegisterDate { get; set; }         // วันที่ลงทะเบียน
        public string? CenterCode { get; set; }             // รหัสศูนย์ (ใช้ CenterCode แทน CenterID)
        public string? CenterName { get; set; }             // ชื่อศูนย์
    }

    public class UpdateCartRequest
    {
        public string ProductID { get; set; }
        public int Quantity { get; set; }
    }

    // ⭐ Update DL/Center Request
    public class UpdateCartDLCenterRequest
    {
        public string? DLCode { get; set; }
        public string? DLName { get; set; }
        public DateTime? RegisterDate { get; set; }
        public string? CenterCode { get; set; }             // ใช้ CenterCode
        public string? CenterName { get; set; }
    }

    // Response Models
    public class CartResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CartData Data { get; set; }
    }

    public class CartData
    {
        public decimal CartID { get; set; }
        public string MemberCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPV { get; set; }
        public decimal ShippingFee { get; set; }
        public int ItemCount { get; set; }
        public int HoursRemaining { get; set; }

        // ⭐ DL และ Center Information (ตรงกับ SP_GetMemberCart)
        public string? CenterCode { get; set; }             // มีอยู่แล้วในตาราง
        public string? CenterName { get; set; }             // ⭐ NEW
        public string? Makerby { get; set; }                // มีอยู่แล้วในตาราง (DL เดิม)
        public string? DLCode { get; set; }                 // ⭐ NEW (DL ใหม่)
        public string? DLName { get; set; }                 // ⭐ NEW
        public DateTime? RegisterDate { get; set; }         // ⭐ NEW
        

        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }

    public class CartItem
    {
        public int CartItemID { get; set; }
        public string ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public decimal PV { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
    }

    public class CheckoutResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string OrderID { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPV { get; set; }
    }
}