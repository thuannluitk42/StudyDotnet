namespace BookApi.Messages
{
	public record SendEmailCommand
	{
		public string To { get; init; } = string.Empty;
		public string Subject { get; init; } = string.Empty;
		public string Body { get; init; } = string.Empty;
	}
}
