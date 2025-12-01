namespace BookApi.Messages
{
	public record BookDeletedEvent
	{
		public int BookId { get; init; }
		public DateTime DeletedAt { get; init; } = DateTime.UtcNow;
	}
}
