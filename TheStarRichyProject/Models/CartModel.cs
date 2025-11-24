using System;
using System.Collections.Generic;

namespace TheStarRichyProject.Models
{
    // Request Models
    public class AddToCartRequest
    {
        public string ProductID { get; set; }  // ต้องเป็น ProductID ไม่ใช่ ProductId
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public decimal PV { get; set; }
        public decimal ShippingFee { get; set; }
        public int Quantity { get; set; }
        public string Makerby { get; set; }


        // ⭐ เพิ่ม DL และ Center Information
        public string DLCode { get; set; }              // รหัสผู้แนะนำ
        public string DLName { get; set; }              // ชื่อผู้แนะนำ
        public DateTime? RegisterDate { get; set; }     // วันที่ลงทะเบียน
        public string CenterCode { get; set; }          // รหัส Hybrid gold
        public string CenterName { get; set; }          // ชื่อ Hybrid gold
    }

    public class UpdateCartRequest
    {
        public string ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal ShippingFee { get; set; }
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
        public int CartID { get; set; }
        public string MemberCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPV { get; set; }
        public decimal TotalBV { get; set; }  // เพิ่ม
        public string? Makerby { get; set; }
        public string? CenterCode { get; set; }
        public string? BillType { get; set; }
        public string? PaymentType { get; set; }
        public string? DeliveryType { get; set; }
        public string? DelevertAddress { get; set; }
        public string? BranchReceive { get; set; }
        public int ItemCount { get; set; }
        public int HoursRemaining { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        // Alias properties สำหรับ JavaScript
        public int TotalItems => ItemCount;
        public decimal TotalPrice => TotalAmount;
        public decimal ShippingFee { get; set; }

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
        public decimal BV { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal => Price * Quantity;
        public decimal TotalPV => PV * Quantity;
        public decimal TotalBV => BV * Quantity;
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