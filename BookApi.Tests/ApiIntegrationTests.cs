using System.Net;
using System.Net.Http.Json;
using BookApi.Models;
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
	}
}
