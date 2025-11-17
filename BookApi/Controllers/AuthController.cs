using BookApi.Models.Dto;
using BookApi.Services;
using Microsoft.AspNetCore.Authorization;
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

		[HttpPost("refresh")]
		public async Task<IActionResult> Refresh([FromBody] Models.Dto.RefreshRequest request)
		{
			try
			{
				var response = await _authService.RefreshTokenAsync(request.RefreshToken);
				SetRefreshTokenCookie(response.RefreshToken);
				return Ok(new { response.AccessToken, response.ExpiresIn });
			}
			catch (UnauthorizedAccessException) { return Unauthorized(); }
		}

		[HttpPost("logout")]
		public async Task<IActionResult> Logout([FromBody] Models.Dto.RefreshRequest request)
		{
			await _authService.LogoutAsync(request.RefreshToken);
			Response.Cookies.Delete("refreshToken");
			return Ok();
		}

		private void SetRefreshTokenCookie(string token)
		{
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = DateTime.UtcNow.AddDays(7)
			};
			Response.Cookies.Append("refreshToken", token, cookieOptions);
		}

		[Authorize(Policy = "RequireAdmin")]
		[HttpGet("admin")]
		public IActionResult AdminOnly() => Ok("Welcome Admin!");

		[Authorize(Policy = "MinimumAge")]
		[HttpGet("adult")]
		public IActionResult AdultOnly() => Ok("You are 18+");

		[Authorize(Policy = "RequireITDepartment")]
		[HttpGet("it")]
		public IActionResult ITOnly() => Ok("IT Department only");

		//[HttpPost("register")]
		//public async Task<IActionResult> Register([FromBody] RegisterDto dto)
		//{
		//	var user = new AppUser
		//	{
		//		UserName = dto.Email,
		//		Email = dto.Email,
		//		FullName = dto.FullName
		//	};

		//	var result = await _userManager.CreateAsync(user, dto.Password);
		//	if (!result.Succeeded)
		//		return BadRequest(result.Errors);

		//	await _userManager.AddToRoleAsync(user, "User");
		//	return Ok("User created");
		//}
	}
}
