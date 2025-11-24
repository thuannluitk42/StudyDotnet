using BookApi.Binders;
using BookApi.Extensions;
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

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] BookForCreationDto dto)
	{
		var validationResult = await _bookService.ValidateBookForCreationAsync(dto);
		if (!validationResult.IsValid)
			return ValidationProblem(validationResult.ToValidationProblemDetails());

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

	[HttpGet("{id:int}")]
	public async Task<IActionResult> Get(int id)
	{
		var book = await _bookService.GetBookByIdAsync(id);
		return book == null ? NotFound() : Ok(book);
	}

	[HttpGet]
	public async Task<ActionResult<List<Book>>> GetAll()
		=> Ok(await _bookService.GetAllBooksAsync());

	[HttpGet("{year:year}")]
	public async Task<IActionResult> GetBooksByYear(int year)
	{
		var books = await _bookService.GetBooksByYearAsync(year);
		return Ok(books);
	}

	[HttpPut("{id:int}")]
	public async Task<IActionResult> Update(int id, [FromBody] BookForUpdateDto dto)
	{
		var validationResult = await _bookService.ValidateBookForUpdateAsync(dto);
		if (!validationResult.IsValid)
			return ValidationProblem(validationResult.ToValidationProblemDetails());

		var existing = await _bookService.GetBookByIdAsync(id);
		if (existing == null) return NotFound();

		if (dto.Title != null) existing.Title = dto.Title;
		if (dto.Author != null) existing.Author = dto.Author;
		if (dto.PublishedYear.HasValue) existing.PublishedYear = dto.PublishedYear.Value;

		await _bookService.UpdateBookAsync(existing);
		return NoContent();
	}

	[HttpDelete("{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		var book = await _bookService.GetBookByIdAsync(id);
		if (book == null) return NotFound();

		await _bookService.DeleteBookAsync(id);
		return NoContent();
	}

	[HttpGet("by-date")]
	public IActionResult GetByDate([ModelBinder(BinderType = typeof(CustomDateBinder))] DateTime date)
	{
		return Ok($"Selected date: {date:yyyy-MM-dd}");
	}

	[HttpGet("admin")]
	[Authorize(Policy = "RequireAdmin")]
	public IActionResult AdminOnly() => Ok("Welcome Admin! Only Admin can see this.");

	[HttpGet("adult")]
	[Authorize(Policy = "MinimumAge")]
	public IActionResult AdultOnly() => Ok("You are 18+! Content for adults.");

	[HttpGet("it")]
	[Authorize(Policy = "RequireITDepartment")]
	public IActionResult ITOnly() => Ok("IT Department only! Sensitive data here.");
}
