using BookApi.Brokers;
using BookApi.Models;
using BookApi.Services;
using Xunit;

namespace BookApi.Tests
{
	public class BookServiceTests
	{
		[Fact]
		public void Should_Add_And_Retrieve_Book()
		{
			var broker = new InMemoryStorageBroker();
			var service = new BookService(broker);
			var book = new Book { Id = 1, Title = "DI Master" };

			// Act
			service.AddBook(book);
			var result = service.GetAllBooks();

			// Assert
			Assert.Contains(book, result);
			Assert.Single(result);
		}
	}
}
