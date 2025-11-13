using BookApi.Models;
using BookApi.Models.Dto;
using FluentValidation.Results;

namespace BookApi.Brokers
{
	public interface IStorageBroker
	{
		Task AddBookAsync(Book book);
		Task<Book?> GetBookByIdAsync(int id);
		Task<List<Book>> GetBooksByYearAsync(int year);
		Task<List<Book>> GetAllBooksAsync();
		Task UpdateBookAsync(Book book);
		Task DeleteBookAsync(int id);
		Task<int> GetNextIdAsync();
	}
}
