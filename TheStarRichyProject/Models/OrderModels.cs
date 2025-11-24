using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TheStarRichyProject.Models
{
    #region Request Models

    /// <summary>
    /// ข้อมูลการจัดส่งและชำระเงิน (Step 2)
    /// </summary>
    public class CheckoutInfoRequest
    {
        [Required(ErrorMessage = "กรุณาเลือกวิธีชำระเงิน")]
        public string PaymentMethod { get; set; } // "PromptPay", "CreditCard"

        [Required(ErrorMessage = "กรุณาเลือกวิธีการจัดส่ง")]
        public string DeliveryMethod { get; set; } // "Delivery", "Pickup"

        public string DeliveryAddressType { get; set; } // "Member", "Card", "Other", "Branch"
        
        public int? AddressID { get; set; }
        
        public CustomAddressInfo? CustomAddress { get; set; }
        public MemberAddressData? MemberAddress { get; set; }
        public MemberFavoriteAddressData? MemberFavorite { get; set; }
        
        public bool SendInvoice { get; set; }
        
        public string BranchCode { get; set; } // ถ้าเลือก Pickup

        public string BranchName { get; set; } = string.Empty;
        public string OrderID { get; set; }
        
    }

    /// <summary>
    /// ที่อยู่แบบกรอกเอง
    /// </summary>
    public class CustomAddressInfo
    {
        [Required(ErrorMessage = "กรุณากรอกที่อยู่")]
        [MaxLength(500)]
        public string AddressLine { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกจังหวัด")]
        [MaxLength(100)]
        public string Province { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกเขต/อำเภอ")]
        [MaxLength(100)]
        public string District { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกแขวง/ตำบล")]
        [MaxLength(100)]
        public string SubDistrict { get; set; }

        [Required(ErrorMessage = "กรุณากรอกรหัสไปรษณีย์")]
        [MaxLength(10)]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "กรุณากรอกเบอร์โทรศัพท์")]
        [Phone(ErrorMessage = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง")]
        [MaxLength(20)]
        public string Phone { get; set; }
    }

    /// <summary>
    /// ข้อมูลการชำระเงิน (Step 3)
    /// </summary>
    public class PaymentRequest
    {
        [Required]
        public string OrderID { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        // สำหรับบัตรเครดิต
        public CreditCardInfo CardInfo { get; set; }
    }

    /// <summary>
    /// ข้อมูลบัตรเครดิต
    /// </summary>
    public class CreditCardInfo
    {
        [Required(ErrorMessage = "กรุณากรอกหมายเลขบัตร")]
        [CreditCard(ErrorMessage = "หมายเลขบัตรไม่ถูกต้อง")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อบนบัตร")]
        [MaxLength(100)]
        public string CardHolderName { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกเดือนหมดอายุ")]
        [Range(1, 12)]
        public string ExpiryMonth { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกปีหมดอายุ")]
        public string ExpiryYear { get; set; }

        [Required(ErrorMessage = "กรุณากรอก CVV")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV ไม่ถูกต้อง")]
        public string CVV { get; set; }
    }

    #endregion

    #region Response Models

    /// <summary>
    /// ผลลัพธ์การบันทึกข้อมูลการจัดส่ง
    /// </summary>
    public class CheckoutInfoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CheckoutInfoData Data { get; set; }
    }

    public class CheckoutInfoData
    {
        public string OrderID { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
    }

    /// <summary>
    /// ข้อมูลสรุปคำสั่งซื้อ
    /// </summary>
    public class OrderSummaryResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public OrderSummaryData Data { get; set; }
    }

    public class OrderSummaryData
    {
        public string OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string MemberCode { get; set; }
        public string MemberName { get; set; }
        public List<OrderItemInfo> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPV { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
        public string PaymentMethod { get; set; }
        public string DeliveryMethod { get; set; }
        public DeliveryInfoData DeliveryInfo { get; set; }
        public string Status { get; set; }
        public bool SendInvoice { get; set; }
    }

    public class OrderItemInfo
    {
        public string ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public decimal PV { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class DeliveryInfoData
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string FullAddress { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string SubDistrict { get; set; }
        public string PostalCode { get; set; }
    }

    /// <summary>
    /// ผลลัพธ์การสร้างการชำระเงิน
    /// </summary>
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public PaymentData Data { get; set; }
    }

    public class PaymentData
    {
        public string PaymentID { get; set; }
        public string OrderID { get; set; }
        public decimal Amount { get; set; }
        
        // สำหรับ PromptPay
        public string QRCode { get; set; }
        public string QRReference { get; set; }
        public DateTime? ExpiryDateTime { get; set; }
        
        // สำหรับบัตรเครดิต
        public string CardBrand { get; set; }
        public string Last4Digits { get; set; }
        public string GatewayTransactionID { get; set; }
    }

    /// <summary>
    /// สถานะการชำระเงิน
    /// </summary>
    public class PaymentStatusResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public PaymentStatusData Data { get; set; }
    }

    /// <summary>
    /// Model สำหรับสถานะการชำระเงิน
    /// </summary>
    public class PaymentStatusData
    {
        /// <summary>
        /// สถานะหลัก: "Pending", "Success", "Failed", "Expired"
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// สถานะการชำระเงิน (อาจเหมือน Status หรือละเอียดกว่า)
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// สถานะของ Order: "Pending", "Paid", "Processing", "Completed"
        /// </summary>
        public string OrderStatus { get; set; }

        /// <summary>
        /// ข้อความอธิบายสถานะ
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// PaymentID
        /// </summary>
        public string PaymentID { get; set; }

        /// <summary>
        /// OrderID
        /// </summary>
        public string OrderID { get; set; }

        /// <summary>
        /// จำนวนเงิน
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// วันที่ชำระเงิน
        /// </summary>
        public DateTime? PaidDate { get; set; }

        /// <summary>
        /// Transaction Reference
        /// </summary>
        public string TransactionRef { get; set; }
    }

    /// <summary>
    /// รายการที่อยู่ของสมาชิก
    /// </summary>
    public class MemberAddressesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<MemberAddressData> Data { get; set; }
    }

    public class ApiMemberAddress
    {
        [JsonProperty("Membercode")]
        public string MemberCode { get; set; }

        [JsonProperty("Address")]
        public string Address { get; set; }

        [JsonProperty("province")]
        public string Province { get; set; }

        [JsonProperty("zipcode")]
        public string ZipCode { get; set; }

        [JsonProperty("tambon")]
        public object Tambon { get; set; }  // เปลี่ยนเป็น object

        [JsonProperty("amphoe")]
        public object Amphoe { get; set; }  // เปลี่ยนเป็น object

        [JsonProperty("M06_X11_HouseNumber")]
        public object HouseNumber { get; set; }
    }

    public class MemberAddressData
    {
        public int AddressID { get; set; }
        public string? AddressType { get; set; }
        public string? AddressTypeDisplay { get; set; }
        public string? AddressLine { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? SubDistrict { get; set; }
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public string? FullAddress { get; set; }
        public bool? IsDefault { get; set; }
    }

    /// <summary>
    /// รายการที่อยู่ของสมาชิก
    /// </summary>
    public class MemberFavoriteAddressesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<MemberFavoriteAddressData> Data { get; set; }
    }

    public class MemberFavoriteAddressData
    {
        public string? Membercode { get; set; }
        public string? ContacNickname { get; set; }
        public string? Contactperson { get; set; }
        public string? Contact_HouseNumber { get; set; }
        public string? Contact_Alley { get; set; }
        public string? Contact_Road { get; set; }
        public string? Contact_Building { get; set; }
        public string? Contact_Floor { get; set; }
        public string? Contact_Other { get; set; }
        public string? Contact_Zipcode { get; set; }
        public int? Contact_TAMBON_ID { get; set; }
        public string? Contact_Phone { get; set; }
        public string? Contact_Province { get; set; }
        public string? Provincename { get; set; }   // M32_X2 as Provincename
        public string? Contact_Primary { get; set; }
    }


    /// <summary>
    /// รายการสาขา
    /// </summary>
    public class BranchesResponse
    {
        public bool Success { get; set; }
        public List<BranchData> Data { get; set; }
    }

    public class BranchData
    {
        [JsonProperty("M39_PX1")]
        public string? BranchCode { get; set; }

        [JsonProperty("M39_X2")]
        public string? BranchName { get; set; }
        public string? Province { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }

    /// <summary>
    /// ผลลัพธ์ทั่วไป
    /// </summary>
    public class OrderResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    #endregion

    public class CenterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CenterData Data { get; set; }
    }
    public class CenterData
    {
        public string CenterID { get; set; }
        public string CenterName { get; set; }
    }

    #region Database Models (สำหรับ API Project)

    /// <summary>
    /// Order entity
    /// </summary>
    public class Order
    {
        public string OrderID { get; set; }
        public string MemberCode { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPV { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
        
        // Payment Info
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? TransactionID { get; set; }
        
        // Delivery Info
        public string? DeliveryMethod { get; set; }
        public string? DeliveryAddressType { get; set; }
        
        // Address
        public int? ShippingAddressID { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingProvince { get; set; }
        public string? ShippingDistrict { get; set; }
        public string? ShippingSubDistrict { get; set; }
        public string? ShippingPostalCode { get; set; }
        public string? ShippingPhone { get; set; }
        
        // Invoice
        public bool SendInvoice { get; set; }
        
        // Status
        public string OrderStatus { get; set; }
        
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    /// <summary>
    /// OrderItem entity
    /// </summary>
    public class OrderItem
    {
        public int OrderItemID { get; set; }
        public string OrderID { get; set; }
        public string ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal PV { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
    }

    /// <summary>
    /// MemberAddress entity
    /// </summary>
    public class MemberAddress
    {
        public int AddressID { get; set; }
        public string? MemberCode { get; set; }
        public string? AddressType { get; set; }
        public string? AddressLine { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? SubDistrict { get; set; }
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// PaymentTransaction entity
    /// </summary>
    public class PaymentTransaction
    {
        public string TransactionID { get; set; }
        public string OrderID { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        
        // PromptPay
        public string QRCode { get; set; }
        public string QRReference { get; set; }
        
        // Credit Card
        public string? CardNumber { get; set; }
        public string? CardBrand { get; set; }
        
        // Gateway
        public string GatewayTransactionID { get; set; }
        public string GatewayResponse { get; set; }
        
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    #endregion

    #region Enums

    public static class PaymentMethods
    {
        public const string PromptPay = "PromptPay";
        public const string CreditCard = "CreditCard";
    }

    public static class DeliveryMethods
    {
        public const string Delivery = "Delivery";
        public const string Pickup = "Pickup";
    }

    public static class AddressTypes
    {
        public const string Member = "Member";
        public const string Card = "Card";
        public const string Other = "Other";
        public const string Branch = "Branch";
    }

    public static class PaymentStatuses
    {
        public const string Pending = "Pending";
        public const string Success = "Success";
        public const string Failed = "Failed";
    }

    public static class OrderStatuses
    {
        public const string Draft = "Draft";
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }

    #endregion
}
