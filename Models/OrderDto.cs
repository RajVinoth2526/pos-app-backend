namespace ClientAppPOSWebAPI.Models
{
    public class OrderDto
    {
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? OrderStatus { get; set; }
        public bool? IsDraft { get; set; }
        public string? Notes { get; set; }
        public DateTime? CompletedDate { get; set; }
    }

    public class CreateOrderDto
    {
        public int? Id { get; set; } // Optional ID for PUT operations
        public string? OrderId { get; set; } // Optional OrderId field
        public int CustomerId { get; set; } = 0; // Default to 0 if not provided
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public string? PaymentMethod { get; set; }
        public string? OrderStatus { get; set; } // Allow setting order status
        public string? PaymentStatus { get; set; } // Allow setting payment status
        public bool IsDraft { get; set; } = false; // Default to false for new orders
        public string? Notes { get; set; }
        public decimal? SubTotal { get; set; } // Optional subtotal
        public decimal? TaxAmount { get; set; } // Optional tax amount
        public decimal? DiscountAmount { get; set; } // Optional discount amount
        public decimal? TotalAmount { get; set; } // Optional total amount
        public bool? IsTakeaway { get; set; } // Optional takeaway flag
        public string? CartDate { get; set; } // Optional cart date
        public DateTime? CreatedAt { get; set; } // Optional creation date
        public DateTime? UpdatedAt { get; set; } // Optional update date
        public List<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();
    }

    public class CreateOrderItemDto
    {
        public int? Id { get; set; } // Optional ID for PUT operations
        public int ProductId { get; set; }
        public string? Name { get; set; } // Product name
        public decimal? Price { get; set; } // Unit price
        public int Quantity { get; set; }
        public decimal? Total { get; set; } // Total amount for this item
        public decimal? Tax { get; set; } // Tax amount
        public decimal? Discount { get; set; } // Discount amount
        public string? Notes { get; set; }
        public object? Product { get; set; } // Full product object (optional)
    }
}
