using BookApi.Messages;
using MassTransit;

namespace BookApi.Consumers
{
	public class BookAnalyticsConsumer : IConsumer<BookCreatedEvent>
	{
		private readonly ILogger<BookAnalyticsConsumer> _logger;

		public BookAnalyticsConsumer(ILogger<BookAnalyticsConsumer> logger)
		{
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<BookCreatedEvent> context)
		{
			var evt = context.Message;

			_logger.LogInformation("📊 Processing analytics for book {BookId}: {Title}",
				evt.BookId, evt.Title);

			// Simulate analytics processing
			await Task.Delay(500);

			_logger.LogInformation("✅ Analytics processed for book {BookId}", evt.BookId);
		}
	}
}
