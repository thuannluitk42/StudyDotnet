using System.Text.Json;
using BookApi.Brokers;
using BookApi.Models;
using BookApi.Models.Dto;
using BookApi.Validators;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Distributed;

namespace BookApi.Services;

public class BookService : IBookService
{
	private readonly IStorageBroker _storage;
	private readonly IDistributedCache _cache;
	private readonly ILogger<BookService> _logger;
	private const string CacheKey = "books_all";
	private const int CacheMinutes = 10;

	public BookService(IStorageBroker storage, IDistributedCache cache, ILogger<BookService> logger)
	{
		_storage = storage ?? throw new ArgumentNullException(nameof(storage));
		_cache = cache ?? throw new ArgumentNullException(nameof(cache));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task AddBookAsync(Book book)
	{
		await _storage.AddBookAsync(book);
		await InvalidateCacheAsync();
	}

	public async Task<Book?> GetBookByIdAsync(int id)
		=> await _storage.GetBookByIdAsync(id);

	public async Task UpdateBookAsync(Book book)
	{
		await _storage.UpdateBookAsync(book);
		await InvalidateCacheAsync();
	}

	public async Task DeleteBookAsync(int id)
	{
		await _storage.DeleteBookAsync(id);
		await InvalidateCacheAsync();
	}

	public async Task<int> GetNextIdAsync()
		=> await _storage.GetNextIdAsync();

	public async Task<List<Book>> GetBooksByYearAsync(int year)
		=> await _storage.GetBooksByYearAsync(year);

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

	public async Task<List<Book>> GetAllBooksAsync()
	{
		try
		{
			var cached = await _cache.GetStringAsync(CacheKey);
			if (!string.IsNullOrEmpty(cached))
			{
				var des = JsonSerializer.Deserialize<List<Book>>(cached);
				if (des != null)
				{
					_logger.LogDebug("Books returned from cache (Count={Count}).", des.Count);
					return des;
				}

				_logger.LogWarning("Cached value existed but deserialization returned null. Key={Key}", CacheKey);
			}
			else
			{
				_logger.LogDebug("Cache miss for key {Key}", CacheKey);
			}

			var books = await _storage.GetAllBooksAsync() ?? new List<Book>();

			var options = new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheMinutes)
			};

			var payload = JsonSerializer.Serialize(books);
			await _cache.SetStringAsync(CacheKey, payload, options);

			_logger.LogDebug("Books cached (Count={Count}) with key {Key}", books.Count, CacheKey);

			return books;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error when accessing cache. Falling back to storage.");
			var fallback = await _storage.GetAllBooksAsync() ?? new List<Book>();
			return fallback;
		}
	}

	public async Task InvalidateCacheAsync()
	{
		try
		{
			await _cache.RemoveAsync(CacheKey);
			_logger.LogDebug("Cache invalidated for key {Key}", CacheKey);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to invalidate cache for key {Key}", CacheKey);
		}
	}
}
