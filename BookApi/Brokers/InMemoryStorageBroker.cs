using BookApi.Models;

namespace BookApi.Brokers;

public class InMemoryStorageBroker : IStorageBroker
{
	private readonly List<Book> _books = new();
	private int _nextId = 1;

	public async Task AddBookAsync(Book book)
	{
		book.Id = _nextId++;
		_books.Add(book);
		await Task.CompletedTask;
	}

	public async Task<Book?> GetBookByIdAsync(int id)
		=> await Task.FromResult(_books.FirstOrDefault(b => b.Id == id));

	public async Task<List<Book>> GetBooksByYearAsync(int year)
		=> await Task.FromResult(_books.Where(b => b.PublishedYear == year).ToList());

	public async Task<List<Book>> GetAllBooksAsync()
		=> await Task.FromResult(_books.ToList());

	public async Task UpdateBookAsync(Book book)
	{
		var existing = _books.FirstOrDefault(b => b.Id == book.Id);
		if (existing != null)
		{
			existing.Title = book.Title;
			existing.Author = book.Author;
			existing.PublishedYear = book.PublishedYear;
		}
		await Task.CompletedTask;
	}

	public async Task DeleteBookAsync(int id)
	{
		var book = _books.FirstOrDefault(b => b.Id == id);
		if (book != null) _books.Remove(book);
		await Task.CompletedTask;
	}

	public async Task<int> GetNextIdAsync()
		=> await Task.FromResult(_nextId);
}
