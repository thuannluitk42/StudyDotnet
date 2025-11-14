using BookApi.Binders;
using BookApi.Models;
using BookApi.Models.Dto;
using BookApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
	private readonly IBookService _bookService;

	public BooksController(IBookService bookService)
	{
		_bookService = bookService;
	}

	// === CREATE ===
	[HttpPost]
	public async Task<IActionResult> Create([FromBody] BookForCreationDto dto)
	{
		var validationResult = await _bookService.ValidateBookForCreationAsync(dto);
		if (!validationResult.IsValid)
		{
			return ValidationProblem(); // TỰ ĐỘNG
		}

		var book = new Book
		{
			Id = await _bookService.GetNextIdAsync(),
			Title = dto.Title,
			Author = dto.Author,
			PublishedYear = dto.PublishedYear
		};

		await _bookService.AddBookAsync(book);
		return CreatedAtAction(nameof(Get), new { id = book.Id }, book);
	}

	// === READ: Get by ID ===
	[HttpGet("{id:int}")]
	public async Task<IActionResult> Get(int id)
	{
		var book = await _bookService.GetBookByIdAsync(id);
		return book == null ? NotFound() : Ok(book);
	}

	// === READ: Get all ===
	[HttpGet]
	public async Task<ActionResult<List<Book>>> GetAll()
		=> Ok(await _bookService.GetAllBooksAsync());

	// === READ: Get by Year (Custom Route Constraint) ===
	[HttpGet("year/{year:year}")]
	public async Task<IActionResult> GetBooksByYear(int year)
	{
		var books = await _bookService.GetBooksByYearAsync(year);
		return Ok(books);
	}

	// === UPDATE ===
	[HttpPut("{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] BookForUpdateDto dto)
	{
		var validationResult = await _bookService.ValidateBookForUpdateAsync(dto);
		if (!validationResult.IsValid)
		{
			return ValidationProblem(); // TỰ ĐỘNG
		}

		var existing = await _bookService.GetBookByIdAsync(id);
		if (existing == null) return NotFound();

		if (dto.Title != null) existing.Title = dto.Title;
		if (dto.Author != null) existing.Author = dto.Author;
		if (dto.PublishedYear.HasValue) existing.PublishedYear = dto.PublishedYear.Value;

		await _bookService.UpdateBookAsync(existing);
		return NoContent();
	}

	// === DELETE ===
	[HttpDelete("{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		var book = await _bookService.GetBookByIdAsync(id);
		if (book == null) return NotFound();

		await _bookService.DeleteBookAsync(id);
		return NoContent();
	}

	// === CUSTOM MODEL BINDER: Test Date Parsing ===
	[HttpGet("by-date")]
	public IActionResult GetByDate([ModelBinder(BinderType = typeof(CustomDateBinder))] DateTime date)
	{
		return Ok($"Selected date: {date:yyyy-MM-dd}");
	}

	[HttpGet("admin")]
	[Authorize(Roles = "Admin")]
	public IActionResult AdminOnly() => Ok("Welcome, Admin!");
}