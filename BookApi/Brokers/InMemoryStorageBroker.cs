using BookApi.Models;

namespace BookApi.Brokers;

public class InMemoryStorageBroker : IStorageBroker
{
	private readonly List<Book> _books = new();
	private int _nextId = 1;

	public Task<List<Book>> GetAllBooksAsync() => Task.FromResult(_books);

	public Task<Book?> GetBookByIdAsync(int id) =>
		Task.FromResult(_books.FirstOrDefault(b => b.Id == id));

	public Task<Book> AddBookAsync(Book book)
	{
		book.Id = _nextId++;
		_books.Add(book);
		return Task.FromResult(book);
	}

	public Task UpdateBookAsync(Book book)
	{
		var existing = _books.FirstOrDefault(b => b.Id == book.Id);
		if (existing != null)
		{
			existing.Title = book.Title;
			existing.Author = book.Author;
			existing.PublishedDate = book.PublishedDate;
		}
		return Task.CompletedTask;
	}

	public Task DeleteBookAsync(int id)
	{
		var book = _books.FirstOrDefault(b => b.Id == id);
		if (book != null) _books.Remove(book);
		return Task.CompletedTask;
	}
}