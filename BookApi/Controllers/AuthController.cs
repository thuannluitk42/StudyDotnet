using BookApi.Models.Dto;
using BookApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookApi.Controllers
{
	[ApiController]
	[Route("api/auth")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto dto)
		{
			try
			{
				var token = await _authService.LoginAsync(dto);
				return Ok(new { token });
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { message = "Invalid credentials" });
			}
		}
	}
}
