using System.ComponentModel.DataAnnotations;

namespace ClientAppPOSWebAPI.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public int OrderId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = "Generated"; // Generated, Sent, Paid, Cancelled
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public string? TermsAndConditions { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Order Order { get; set; } = null!;
        public List<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }

    public class InvoiceItem
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ProductDescription { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public Invoice Invoice { get; set; } = null!;
    }

    public class InvoiceDto
    {
        public int? Id { get; set; }
        public string? InvoiceNumber { get; set; }
        public int OrderId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Status { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Notes { get; set; }
        public string? TermsAndConditions { get; set; }
        public List<InvoiceItemDto>? InvoiceItems { get; set; }
    }

    public class InvoiceItemDto
    {
        public int? Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public decimal? UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class InvoiceTemplate
    {
        public string CompanyName { get; set; } = null!;
        public string CompanyAddress { get; set; } = null!;
        public string CompanyPhone { get; set; } = null!;
        public string CompanyEmail { get; set; } = null!;
        public string? CompanyLogo { get; set; }
        public string? TaxNumber { get; set; }
        public string? Website { get; set; }
        public string Currency { get; set; } = "USD";
        public string? FooterText { get; set; }
        public string? TermsAndConditions { get; set; }
    }

    public class InvoicePrintRequest
    {
        public int InvoiceId { get; set; }
        public string Format { get; set; } = "PDF"; // PDF, HTML
        public bool IncludeLogo { get; set; } = true;
        public bool ShowTaxBreakdown { get; set; } = true;
        public string? CustomFooter { get; set; }
    }
}

