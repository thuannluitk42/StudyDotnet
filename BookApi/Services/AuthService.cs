using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookApi.Data;
using BookApi.Models;
using BookApi.Models.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
namespace BookApi.Services
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly AppDbContext _context;
		private readonly IConfiguration _config;

		public AuthService(
			UserManager<AppUser> userManager,
			AppDbContext context,
			IConfiguration config)
		{
			_userManager = userManager;
			_context = context;
			_config = config;
		}

		// ==================================================================
		// 1. LOGIN → ACCESS + REFRESH TOKEN + CUSTOM CLAIMS
		// ==================================================================
		public async Task<AuthResponse> LoginAsync(LoginDto dto)
		{
			var user = await _userManager.FindByEmailAsync(dto.Email);

			if (user == null)
				throw new UnauthorizedAccessException("Invalid credentials");

			var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
			if (!isPasswordValid)
				throw new UnauthorizedAccessException("Invalid credentials");

			var (accessToken, expires) = await GenerateAccessTokenAsync(user);
			var refreshToken = GenerateRefreshToken();

			await SaveRefreshTokenAsync(user.Id, refreshToken);

			return new AuthResponse
			{
				AccessToken = accessToken,
				ExpiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds,
				RefreshToken = refreshToken
			};
		}

		// ==================================================================
		// 2. REFRESH TOKEN → CẤP MỚI ACCESS + REFRESH
		// ==================================================================
		public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
		{
			var rt = await _context.RefreshTokens
				.FirstOrDefaultAsync<RefreshToken>(t =>
					t.Token == refreshToken &&
					!t.IsRevoked &&
					t.Expires > DateTime.UtcNow);

			if (rt == null)
				throw new UnauthorizedAccessException("Invalid or expired refresh token");

			var user = await _userManager.FindByIdAsync(rt.UserId);
			if (user == null)
				throw new UnauthorizedAccessException("User not found");

			rt.IsRevoked = true;

			var (newAccessToken, newExpires) = await GenerateAccessTokenAsync(user);
			var newRefreshToken = GenerateRefreshToken();

			await SaveRefreshTokenAsync(user.Id, newRefreshToken);
			await _context.SaveChangesAsync();

			return new AuthResponse
			{
				AccessToken = newAccessToken,
				ExpiresIn = (int)(newExpires - DateTime.UtcNow).TotalSeconds,
				RefreshToken = newRefreshToken
			};
		}

		// ==================================================================
		// 3. LOGOUT → REVOKE REFRESH TOKEN
		// ==================================================================
		public async Task LogoutAsync(string refreshToken)
		{
			var rt = await _context.RefreshTokens
				.FirstOrDefaultAsync(t => t.Token == refreshToken);

			if (rt != null)
			{
				rt.IsRevoked = true;
				await _context.SaveChangesAsync();
			}
		}

		// ==================================================================
		// HELPER: TẠO ACCESS TOKEN VỚI CUSTOM CLAIMS (DÙNG CHO POLICY)
		// ==================================================================
		private async Task<(string token, DateTime expires)> GenerateAccessTokenAsync(AppUser user)
		{
			var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, user.Id),
			new Claim(ClaimTypes.Email, user.Email!),
			new Claim(ClaimTypes.Name, user.FullName ?? user.UserName!),
            // CUSTOM CLAIMS CHO POLICY
            new Claim("age", "25"),
			new Claim("department", "IT")
		};

			var roles = await _userManager.GetRolesAsync(user);
			claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var expires = DateTime.UtcNow.AddMinutes(60);

			var token = new JwtSecurityToken(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
				claims: claims,
				expires: expires,
				signingCredentials: creds
			);

			return (new JwtSecurityTokenHandler().WriteToken(token), expires);
		}

		// ==================================================================
		// HELPER: TẠO REFRESH TOKEN (64 BYTES)
		// ==================================================================
		private string GenerateRefreshToken()
		{
			var randomBytes = RandomNumberGenerator.GetBytes(64);
			return Convert.ToBase64String(randomBytes);
		}

		// ==================================================================
		// HELPER: LƯU REFRESH TOKEN VÀO DB
		// ==================================================================
		private async Task SaveRefreshTokenAsync(string userId, string token)
		{
			var refreshToken = new RefreshToken
			{
				UserId = userId,
				Token = token,
				Expires = DateTime.UtcNow.AddDays(7),
				IsRevoked = false
			};

			_context.RefreshTokens.Add(refreshToken);
			await _context.SaveChangesAsync();
		}

		//public async Task<string> LoginAsync(LoginDto dto)
		//{
		//	if (dto.Email != "studydotnet@yopmail.com" || dto.Password != "Abc12345@") 
		//		throw new UnauthorizedAccessException("Invalid credentials");

		//	var claims = new[]
		//	{
		//	new Claim(ClaimTypes.NameIdentifier, "1"),
		//	new Claim(ClaimTypes.Email, dto.Email),
		//	new Claim(ClaimTypes.Role, "Admin")
		//};

		//	var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
		//	var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		//	var token = new JwtSecurityToken(
		//		issuer: _config["Jwt:Issuer"],
		//		audience: _config["Jwt:Audience"],
		//		claims: claims,
		//		expires: DateTime.Now.AddMinutes(60),
		//		signingCredentials: creds
		//	);

		//	return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
		//}
	}
}
