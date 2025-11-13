using BookApi.Brokers;
using BookApi.Models;
using BookApi.Models.Dto;
using BookApi.Validators;
using FluentValidation.Results;

namespace BookApi.Services;

public class BookService : IBookService
{
	private readonly IStorageBroker _storage;

	public BookService(IStorageBroker storage)
	{
		_storage = storage;
	}

	// === CRUD ===
	public async Task AddBookAsync(Book book)
		=> await _storage.AddBookAsync(book);

	public async Task<Book?> GetBookByIdAsync(int id)
		=> await _storage.GetBookByIdAsync(id);

	public async Task<List<Book>> GetAllBooksAsync()
		=> await _storage.GetAllBooksAsync();

	public async Task UpdateBookAsync(Book book)
		=> await _storage.UpdateBookAsync(book);

	public async Task DeleteBookAsync(int id)
		=> await _storage.DeleteBookAsync(id);

	public async Task<int> GetNextIdAsync()
		=> await _storage.GetNextIdAsync();

	public async Task<List<Book>> GetBooksByYearAsync(int year)
		=> await _storage.GetBooksByYearAsync(year);

	// === VALIDATION: Mabrouk’s Pattern ===
	public async Task<ValidationResult> ValidateBookForCreationAsync(BookForCreationDto dto)
	{
		var validator = new BookForCreationDtoValidator();
		var result = await validator.ValidateAsync(dto);
		return result;
	}

	public async Task<ValidationResult> ValidateBookForUpdateAsync(BookForUpdateDto dto)
	{
		var validator = new BookForUpdateDtoValidator();
		var result = await validator.ValidateAsync(dto);
		return result;
	}
}