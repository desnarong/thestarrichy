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
    /// รายการสินค้าสำหรับ TVP dbo.SalesDetailType
    /// </summary>
    public class SyncOrderDetail
    {
        public string ProductID { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal PV { get; set; }
        public int Qty { get; set; }
        public string BillNo { get; set; } = string.Empty;
        public string ProductSet { get; set; } = string.Empty;
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
