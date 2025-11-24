using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace BookApi.Services
{
	public class RabbitMqService : IRabbitMqService, IDisposable
	{
		private readonly IConnection _connection;
		private readonly IModel _channel;
		private readonly ILogger<RabbitMqService> _logger;

		public RabbitMqService(IConfiguration configuration, ILogger<RabbitMqService> logger)
		{
			_logger = logger;

			var factory = new ConnectionFactory
			{
				HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
				Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
				UserName = configuration["RabbitMQ:UserName"] ?? "guest",
				Password = configuration["RabbitMQ:Password"] ?? "guest"
			};

			try
			{
				_connection = factory.CreateConnection();
				_channel = _connection.CreateModel();
				_logger.LogInformation("Connected to RabbitMQ");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to connect to RabbitMQ");
				throw;
			}
		}

		public void PublishMessage(string queueName, string message)
		{
			_channel.QueueDeclare(
				queue: queueName,
				durable: true,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			var body = Encoding.UTF8.GetBytes(message);
			var properties = _channel.CreateBasicProperties();
			properties.Persistent = true;

			_channel.BasicPublish(
				exchange: "",
				routingKey: queueName,
				basicProperties: properties,
				body: body
			);

			_logger.LogInformation("Published message to queue {Queue}: {Message}", queueName, message);
		}

		public void PublishBookCreated(int bookId, string title)
		{
			var message = JsonSerializer.Serialize(new
			{
				EventType = "BookCreated",
				BookId = bookId,
				Title = title,
				Timestamp = DateTime.UtcNow
			});

			PublishMessage("book-events", message);
		}

		public void PublishBookDeleted(int bookId)
		{
			var message = JsonSerializer.Serialize(new
			{
				EventType = "BookDeleted",
				BookId = bookId,
				Timestamp = DateTime.UtcNow
			});

			PublishMessage("book-events", message);
		}

		public void Dispose()
		{
			_channel?.Close();
			_connection?.Close();
			_logger.LogInformation("Disconnected from RabbitMQ");
		}
	}
}
