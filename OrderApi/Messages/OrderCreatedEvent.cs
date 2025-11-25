namespace OrderApi.Messages
{
    public record OrderCreatedEvent
    {
        public int OrderId { get; init; }
        public int BookId { get; init; }
        public string BookTitle { get; init; } = string.Empty;
        public string BookAuthor { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal TotalPrice { get; init; }
        public string CustomerEmail { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }
}