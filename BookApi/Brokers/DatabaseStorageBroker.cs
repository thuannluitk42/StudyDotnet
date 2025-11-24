using BookApi.Data;
using BookApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BookApi.Brokers;

public class DatabaseStorageBroker : IStorageBroker
{
	private readonly AppDbContext _context;

	public DatabaseStorageBroker(AppDbContext context)
	{
		_context = context;
	}

	public async Task AddBookAsync(Book book)
	{
		_context.Books.Add(book);
		await _context.SaveChangesAsync();
	}

	public async Task<Book?> GetBookByIdAsync(int id)
		=> await _context.Books.FindAsync(id);

	public async Task<List<Book>> GetBooksByYearAsync(int year)
		=> await _context.Books.Where(b => b.PublishedYear == year).ToListAsync();

	public async Task<List<Book>> GetAllBooksAsync()
		=> await _context.Books.ToListAsync();

	public async Task UpdateBookAsync(Book book)
	{
		_context.Books.Update(book);
		await _context.SaveChangesAsync();
	}

	public async Task DeleteBookAsync(int id)
	{
		var book = await _context.Books.FindAsync(id);
		if (book != null)
		{
			_context.Books.Remove(book);
			await _context.SaveChangesAsync();
		}
	}

	public async Task<int> GetNextIdAsync()
	{
		var maxId = await _context.Books.MaxAsync(b => (int?)b.Id) ?? 0;
		return maxId + 1;
	}
}