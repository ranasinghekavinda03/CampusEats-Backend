using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CampusEats.Api.Data;
using CampusEats.Api.Dtos;
using CampusEats.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CampusEats.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public AuthService(
        AppDbContext db,
        IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (existingUser != null)
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "Customer",
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);

        await _db.SaveChangesAsync();
    }

    public async Task<string?> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
            return null;

        var validPassword = BCrypt.Net.BCrypt.Verify(
            dto.Password,
            user.PasswordHash);

        if (!validPassword)
            return null;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var secret = _configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("JWT key is missing.");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secret));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}