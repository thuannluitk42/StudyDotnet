using System.Net;
using System.Net.Http.Json;
using BookApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BookApi.Tests
{
	public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>  // ← ĐÂY LÀ NƠI _factory ĐẾN
	{
		private readonly WebApplicationFactory<Program> _factory;  // ← KHAI BÁO

		public ApiIntegrationTests(WebApplicationFactory<Program> factory)
		{
			_factory = factory;
		}

		[Fact]
		public async Task Get_By_Id_Should_Return_Book()
		{
			var client = _factory.CreateClient();
			var response = await client.GetAsync("/api/books/1");
			response.EnsureSuccessStatusCode();
			var book = await response.Content.ReadFromJsonAsync<Book>();
			Assert.Equal(1, book?.Id);
		}

		[Fact]
		public async Task Get_Year_Invalid_Should_404()
		{
			var client = _factory.CreateClient();
			var response = await client.GetAsync("/api/books/year/1800");
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Fact]
		public async Task Create_Invalid_Year_Should_400()
		{
			var client = _factory.CreateClient();
			var dto = new { title = "Test", author = "A", publishedYear = 1800 };
			var response = await client.PostAsJsonAsync("/api/books", dto);
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

			var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
			Assert.Contains("Published year", problem?.Errors.Values.SelectMany(v => v).First());
		}

		[Fact]
		public async Task Update_Valid_Should_204()
		{
			var client = _factory.CreateClient();
			await client.PostAsJsonAsync("/api/books", new { title = "Old", author = "A", publishedYear = 2000 });

			var updateDto = new { title = "New Title" };
			var response = await client.PutAsJsonAsync("/api/books/1", updateDto);
			Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
		}
	}
}
