using Microsoft.AspNetCore.Mvc;
using BookApi.Models;
using BookApi.Services;
using ILogger = BookApi.Services.ILogger;

namespace BookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
	private readonly IBookService _bookService;
	private readonly ILogger _logger;

	public BooksController(IBookService bookService, ILogger logger)
	{
		_bookService = bookService;
		_logger = logger;
	}

	[HttpGet]
	public async Task<ActionResult<List<Book>>> GetAll()
		=> Ok(await _bookService.GetAllBooksAsync());

	[HttpGet("{id}")]
	public async Task<ActionResult<Book>> Get(int id)
	{
		var book = await _bookService.GetBookByIdAsync(id);
		return book is null ? NotFound() : Ok(book);
	}

	[HttpPost]
	public async Task<ActionResult<Book>> Create(Book book)
	{
		_logger.Log($"Adding book: {book.Title}");
		var created = await _bookService.AddBookAsync(book);
		return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(int id, Book book)
	{
		if (id != book.Id) return BadRequest();
		await _bookService.UpdateBookAsync(book);
		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(int id)
	{
		await _bookService.DeleteBookAsync(id);
		return NoContent();
	}
}