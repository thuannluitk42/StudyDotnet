using OrderApi.Messages;
using MassTransit;

namespace BookApi.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedEventConsumer> _logger;

        public OrderCreatedEventConsumer(ILogger<OrderCreatedEventConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "📦 [BookService] Received OrderCreatedEvent: OrderId={OrderId}, BookId={BookId}, Quantity={Quantity}",
                evt.OrderId, evt.BookId, evt.Quantity);

            // Simulate stock update
            _logger.LogInformation(
                "📉 [BookService] Updating stock for BookId={BookId}: -{Quantity} copies",
                evt.BookId, evt.Quantity);

            await Task.Delay(100); // Simulate processing

            _logger.LogInformation(
                "✅ [BookService] Stock updated successfully for BookId={BookId}",
                evt.BookId);
        }
    }
}
