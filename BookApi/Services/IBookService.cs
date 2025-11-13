using BookApi.Models;

namespace BookApi.Services;

public interface IBookService
{
	Task<List<Book>> GetAllBooksAsync();
	Task<Book?> GetBookByIdAsync(int id);
	Task<Book> AddBookAsync(Book book);
	Task<Book> UpdateBookAsync(Book book);
	Task DeleteBookAsync(int id);

	Book? GetById(int id);
	List<Book> GetBooksByYear(int year);
}