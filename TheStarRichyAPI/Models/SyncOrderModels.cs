namespace TheStarRichyApi.Models
{
    /// <summary>
    /// Request model สำหรับ sync order ไป T05
    /// </summary>
    public class SyncOrderToT05Request
    {
        public string SaleNo { get; set; } = string.Empty;
        public string Membercode { get; set; } = string.Empty;
        public string Billtype { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; } = DateTime.Now;
        public string CreateBy { get; set; } = string.Empty;
        public decimal PV { get; set; }
        public decimal Amount { get; set; }
        public List<SyncOrderDetail> Detail { get; set; } = new();
    }

    /// <summary>
    /// รายการสินค้าสำหรับ TVP dbo.SalesDetailType (15 columns ตรงตาม DB)
    /// </summary>
    public class SyncOrderDetail
    {
        public int SeqNo { get; set; }
        public string SaleID { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; } = DateTime.Now;
        public string Billtype { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public decimal Price_unit { get; set; }
        public decimal PV_unit { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPV { get; set; }
        public string CreateBy { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string ReferorderID { get; set; } = string.Empty;
        public string Mainset { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response จาก SP_SyncOrderToT05
    /// </summary>
    public class SyncOrderToT05Response
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string OrderID { get; set; } = string.Empty;
    }
}
