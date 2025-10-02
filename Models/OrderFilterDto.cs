namespace ClientAppPOSWebAPI.Models
{
    public class OrderFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? OrderNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public bool? IsDraft { get; set; } // Filter by draft status
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinTotal { get; set; }
        public decimal? MaxTotal { get; set; }
        public DateTimeOffset? OrderStartDate { get; set; }
        public DateTimeOffset? OrderEndDate { get; set; }

    }
}
