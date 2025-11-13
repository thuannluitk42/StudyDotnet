using BookApi.Models;
using BookApi.Models.Dto;
using FluentValidation.Results;

namespace BookApi.Services;

/// <summary>
/// Service interface for managing books.
/// Follows The Standard: Broker → Service → Controller
/// </summary>
public interface IBookService
{
	// === CRUD Operations ===
	Task AddBookAsync(Book book);
	Task<Book?> GetBookByIdAsync(int id);
	Task<List<Book>> GetBooksByYearAsync(int year);
	Task<List<Book>> GetAllBooksAsync();
	Task UpdateBookAsync(Book book);
	Task DeleteBookAsync(int id);
	Task<int> GetNextIdAsync();

	// === Validation (Mabrouk’s Pattern - p.168) ===
	Task<ValidationResult> ValidateBookForCreationAsync(BookForCreationDto dto);
	Task<ValidationResult> ValidateBookForUpdateAsync(BookForUpdateDto dto);
}