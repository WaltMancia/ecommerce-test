
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecureShop.Application.Services;
using SecureShop.Infrastructure.Data;
using SecureShop.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de la base de datos MySQL
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se ha configurado la cadena de conexión DefaultConnection.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 2, 0))));

// 2. Inyección de dependencias
builder.Services.AddScoped<IAuthService, AuthService>();

// 3. Validación de la configuración JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSettings["Key"];
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key debe estar configurada y tener al menos 32 bytes UTF-8.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "Debes configurar Jwt:Issuer y Jwt:Audience.");
}

// 4. Autenticación mediante JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)),

                // Rechazar tokens con expiración futura excesiva
                // y limitar la tolerancia del reloj.
                ClockSkew = TimeSpan.FromSeconds(30)
            };
    });

// 5. Autorización y controladores
builder.Services.AddAuthorization();
builder.Services.AddControllers();

// 6. Documentación de API con Swagger / Swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 7. Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// La autenticación debe ejecutarse antes de la autorización.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();