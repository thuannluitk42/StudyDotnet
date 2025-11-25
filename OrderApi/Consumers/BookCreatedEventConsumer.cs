using MassTransit;
using BookApi.Messages;  // Đổi từ OrderApi.Messages → BookApi.Messages

namespace OrderApi.Consumers
{
    public class BookCreatedEventConsumer : IConsumer<BookCreatedEvent>
    {
        private readonly ILogger<BookCreatedEventConsumer> _logger;

        public BookCreatedEventConsumer(ILogger<BookCreatedEventConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookCreatedEvent> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "📚 [OrderService] Received BookCreatedEvent: BookId={BookId}, Title={Title}, Author={Author}",
                evt.BookId, evt.Title, evt.Author);

            // Simulate processing - có thể cache book info để tạo order sau
            await Task.Delay(100);

            _logger.LogInformation(
                "✅ [OrderService] Book info cached for future orders: {Title}",
                evt.Title);
        }
    }
}