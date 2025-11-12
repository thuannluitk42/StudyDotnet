using BookApi.Models;

namespace BookApi.Brokers
{
	public interface IStorageBroker
	{
		Task<List<Book>> GetAllBooksAsync();
		Task<Book?> GetBookByIdAsync(int id);
		Task<Book> AddBookAsync(Book book);
		Task UpdateBookAsync(Book book);
		Task DeleteBookAsync(int id);
	}
}
