using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheStarRichyProject.Models
{
    /// <summary>
    /// Model สำหรับกลุ่มสินค้า (Product Group)
    /// </summary>
    public class ProductGroup
    {
        public string? ProductGroupCode { get; set; }
        public string? ProductGroupThaiName { get; set; }
        public string? ProductGroupEngName { get; set; }
    }

    /// <summary>
    /// Model สำหรับสินค้า
    /// </summary>
    // Custom Converter สำหรับจัดการ string หรือ object
    public class StringOrObjectConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Skip the object
                JsonDocument.ParseValue(ref reader);
                return null;
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            throw new System.Text.Json.JsonException($"Unexpected token type: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value);
            }
        }
    }

    // Model
    public class Product
    {
        [JsonPropertyName("ProductID")]
        public string? ProductId { get; set; }
        [JsonPropertyName("ProductThaiName")]
        public string? ProductName { get; set; }
        [JsonPropertyName("ProductEngName")]
        public string? ProductNameEn { get; set; }
        [JsonPropertyName("ProductDescription")]
        public string? Description { get; set; }
        [JsonPropertyName("ProductGroupcode")]
        public string? GroupCode { get; set; }
        [JsonPropertyName("ProductGroupThaiName")]
        public string? GroupName { get; set; }
        [JsonPropertyName("MemberPrice")]
        public decimal Price { get; set; }
        [JsonPropertyName("PV")]
        public decimal PV { get; set; }
        [JsonPropertyName("BV")]
        public decimal BV { get; set; }
        [JsonPropertyName("LimitPerMembercode")]
        public string? LimitPerMembercode { get; set; }
        [JsonPropertyName("Totalproduct")]
        public int StockQuantity { get; set; }
        [JsonPropertyName("Picture1")]
        [JsonConverter(typeof(StringOrObjectConverter))]
        public string? ImageUrl { get; set; }
        [JsonPropertyName("Picture2")]
        [JsonConverter(typeof(StringOrObjectConverter))]
        public string? ImageUrl2 { get; set; }
        [JsonPropertyName("Picture3")]
        [JsonConverter(typeof(StringOrObjectConverter))]
        public string? ImageUrl3 { get; set; }
        [JsonPropertyName("Picture4")]
        [JsonConverter(typeof(StringOrObjectConverter))]
        public string? ImageUrl4 { get; set; }
        [JsonPropertyName("Picture5")]
        [JsonConverter(typeof(StringOrObjectConverter))]
        public string? ImageUrl5 { get; set; }
        [JsonPropertyName("Topupcheck")]
        public string? TopupCheck { get; set; }
        [JsonPropertyName("UnitOfProduct_th")]
        public string? Unit { get; set; }

        [JsonPropertyName("Membercode")]
        [JsonConverter(typeof(StringOrObjectConverter))]
        public string? Membercode { get; set; }

        [JsonPropertyName("TotalbuyPerson")]
        public int TotalbuyPerson { get; set; }

        [JsonPropertyName("TotalbuyALL")]
        public int TotalbuyALL { get; set; }

        [JsonPropertyName("TypeofFee")]
        public string? TypeofFee { get; set; }

        [JsonPropertyName("CondFee")]
        public decimal? CondFee { get; set; }

        [JsonPropertyName("DeleveryFe1")]
        public decimal? DeliveryFee1 { get; set; }

        [JsonPropertyName("DeleveryFee2")]
        public decimal? DeliveryFee2 { get; set; }
        public bool IsActive => TopupCheck == "2";
        public bool IsTopup => TopupCheck == "2";
    }

    ///// <summary>
    ///// Model สำหรับรายการสินค้าใน Cart
    ///// </summary>
    //public class CartItem
    //{
    //    public int ProductId { get; set; }
    //    public string? ProductCode { get; set; }
    //    public string? ProductName { get; set; }
    //    public decimal Price { get; set; }
    //    public decimal PV { get; set; }
    //    public decimal BV { get; set; }
    //    public int Quantity { get; set; }
    //    public decimal TotalPrice => Price * Quantity;
    //    public decimal TotalPV => PV * Quantity;
    //    public decimal TotalBV => BV * Quantity;
    //}

    /// <summary>
    /// Model สำหรับ Member ที่ใช้ซื้อสินค้า
    /// </summary>
    public class MemberForSale
    {
        public string? Membercode { get; set; }
        public string? DLcode { get; set; }
        public string? DlName { get; set; }
        public DateTime? RegisterDate { get; set; }
    }

    /// <summary>
    /// ViewModel สำหรับหน้า Buy Personal Order
    /// </summary>
    public class BuyPersonalOrderViewModel
    {
        public string? Membercode { get; set; }
        public List<ProductGroup> ProductGroups { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        public List<CartItem> CartItems { get; set; } = new();
        public MemberForSale CurrentMember { get; set; }

        //// Summary
        public decimal TotalPrice => CartItems.Sum(x => x.SubTotal);
        public decimal TotalPV => CartItems.Sum(x => x.PV);
        public decimal TotalBV => CartItems.Sum(x => x.PV);
        public decimal ShippingFee()
        {
            var cartitem = CartItems.FirstOrDefault();
            if (cartitem != null)
            {
                var product = Products.FirstOrDefault(x => x.ProductId == cartitem.ProductID);
                if (product.TypeofFee == "1")
                {
                    if (CartItems.Sum(x => x.PV) > product.CondFee) return 0;
                    else return product.DeliveryFee1 ?? 0;
                }
                else
                {
                    if (CartItems.Sum(x => x.SubTotal) > product.CondFee) return 0;
                    else return product.DeliveryFee1 ?? 0;
                }
            }
            return 0;
        }
        public int TotalItems => CartItems.Sum(x => x.Quantity);

        // Filter
        public string? SelectedGroupCode { get; set; }
        public string? SearchKeyword { get; set; }
    }
}
