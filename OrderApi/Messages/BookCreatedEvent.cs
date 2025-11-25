namespace BookApi.Messages
{
    public record BookCreatedEvent
    {
        public int BookId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public int PublishedYear { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }
}