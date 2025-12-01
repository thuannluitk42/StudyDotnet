namespace BookApi.Services
{
    public interface IRabbitMqService
    {
		void PublishMessage(string queueName, string message);
		void PublishBookCreated(int bookId, string title);
		void PublishBookDeleted(int bookId);
	}
}
