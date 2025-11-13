using BookApi.Brokers;
using BookApi.Models;

namespace BookApi.Services;

public class BookService : IBookService
{
	private readonly IStorageBroker _storageBroker;

	public BookService(IStorageBroker storageBroker)
	{
		_storageBroker = storageBroker;
	}

	public async Task<List<Book>> GetAllBooksAsync() =>
		await _storageBroker.GetAllBooksAsync();

	public async Task<Book?> GetBookByIdAsync(int id) =>
		await _storageBroker.GetBookByIdAsync(id);

	public async Task<Book> AddBookAsync(Book book)
	{
		if (string.IsNullOrWhiteSpace(book.Title))
			throw new ArgumentException("Title is required.");

		return await _storageBroker.AddBookAsync(book);
	}

	public async Task<Book> UpdateBookAsync(Book book)
	{
		if (string.IsNullOrWhiteSpace(book.Title))
			throw new ArgumentException("Title is required.");

		await _storageBroker.UpdateBookAsync(book);
		return book;
	}

	public async Task DeleteBookAsync(int id) =>
		await _storageBroker.DeleteBookAsync(id);

	public Book? GetById(int id)
	{
		return _storageBroker.GetById(id);
	}

	public List<Book> GetBooksByYear(int year)
	{
		return _storageBroker.GetBooksByYear(year);
	}
}