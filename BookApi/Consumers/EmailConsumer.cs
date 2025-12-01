using BookApi.Messages;
using MassTransit;

namespace BookApi.Consumers
{
	public class EmailConsumer : IConsumer<SendEmailCommand>
	{
		private readonly ILogger<EmailConsumer> _logger;

		public EmailConsumer(ILogger<EmailConsumer> logger)
		{
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<SendEmailCommand> context)
		{
			var message = context.Message;

			_logger.LogInformation("?? Sending email to {To}: {Subject}",
				message.To, message.Subject);

			// Simulate email sending delay
			await Task.Delay(1000);

			// Simulate random failure for retry testing (20% failure rate)
			if (Random.Shared.Next(0, 10) < 2)
			{
				_logger.LogError("? Failed to send email (simulated failure)");
				throw new Exception("Email service unavailable");
			}

			_logger.LogInformation("? Email sent successfully to {To}", message.To);
		}
	}
}
