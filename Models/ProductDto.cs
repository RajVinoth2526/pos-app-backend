namespace ClientAppPOSWebAPI.Models
{
    public class ProductDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; }
        public string? Image { get; set; }
        public decimal Price { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public string? Category { get; set; }
        public int? InitialQuantity { get; set; }
        public int? StockQuantity { get; set; }
        public string? UnitType { get; set; } // e.g., volume, weight
        public string? Unit { get; set; } // e.g., pcs, kg
        public double? UnitValue { get; set; } 
        public float? TaxRate { get; set; } // percentage
        public float? DiscountRate { get; set; } // percentage
        public bool IsAvailable { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
