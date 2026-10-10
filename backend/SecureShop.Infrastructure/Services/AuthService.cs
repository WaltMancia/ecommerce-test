
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SecureShop.Application.DTOs;
using SecureShop.Application.Services;
using SecureShop.Domain.Entities;
using SecureShop.Domain.Enums;
using SecureShop.Infrastructure.Data;

namespace SecureShop.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(
        AppDbContext context,
        IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
    {
        var email = request.Email.Trim();

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == email);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "El email ya está registrado.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                request.Password,
                workFactor: 11),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = Role.Customer
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return new AuthResponse(
            token,
            user.Email,
            user.Role.ToString());
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request)
    {
        var email = request.Email.Trim();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        // No revelar si el correo o la contraseña son incorrectos.
        if (user is null ||
            !BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash) ||
            !user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Credenciales inválidas.");
        }

        var token = GenerateJwtToken(user);

        return new AuthResponse(
            token,
            user.Email,
            user.Role.ToString());
    }

    private string GenerateJwtToken(User user)
    {
        var key = _config["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "La configuración Jwt:Key no está definida.");
        }

        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "La configuración Jwt:Issuer y Jwt:Audience es obligatoria.");
        }

        // HS256 necesita una clave suficientemente robusta.
        if (Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key debe tener al menos 32 bytes UTF-8.");
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new Claim(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                user.Role.ToString()),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}