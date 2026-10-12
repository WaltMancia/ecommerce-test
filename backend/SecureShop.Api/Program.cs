using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecureShop.Application.Services;
using SecureShop.Infrastructure.Data;
using SecureShop.Infrastructure.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de la base de datos MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("No se ha configurado la cadena de conexión DefaultConnection.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 2, 0))));

// 2. Inyección de dependencias
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>(); // Nota: Eliminé la duplicación que había de esta línea

builder.Services.AddHttpClient<PaymentGatewayClient>();
builder.Services.AddHttpClient<NotificationClient>();

// 3. Seguridad: CORS Estricto (Mitiga CSRF y permite conexión desde React)
builder.Services.AddCors(options =>
{
    options.AddPolicy("StrictPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Origen del frontend en Vite
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 4. Seguridad: Rate Limiting (Mitiga DoS y Fuerza Bruta)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("GlobalLimit", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 100; // Máximo 100 peticiones cada 10 segundos por IP
        opt.QueueLimit = 2;
    });
});

// 5. Validación de Entradas (FluentValidation)
builder.Services.AddValidatorsFromAssemblyContaining<SecureShop.Application.Validators.RegisterRequestValidator>();

// 6. Validación de la configuración JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:Key debe estar configurada y tener al menos 32 bytes UTF-8.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException("Debes configurar Jwt:Issuer y Jwt:Audience.");
}

// 7. Autenticación mediante JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            // Rechazar tokens con expiración futura excesiva y limitar tolerancia del reloj
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// 8. Autorización y controladores
builder.Services.AddAuthorization();
builder.Services.AddControllers();

// 9. Documentación de API con Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- PIPELINE HTTP ---

// 10. Seguridad: HTTP Response Headers (Mitiga XSS, Clickjacking, MIME-Sniffing)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self' http://localhost:5173");
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Aplicar políticas de red y seguridad
app.UseCors("StrictPolicy");
app.UseRateLimiter();

// La autenticación debe ejecutarse antes de la autorización.
app.UseAuthentication();
app.UseAuthorization();

// Mapear endpoints y aplicar limitador de tasa global
app.MapControllers().RequireRateLimiting("GlobalLimit");

app.Run();