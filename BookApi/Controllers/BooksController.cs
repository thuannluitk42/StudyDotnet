using Microsoft.AspNetCore.Mvc;
using BookApi.Models;
using BookApi.Services;

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