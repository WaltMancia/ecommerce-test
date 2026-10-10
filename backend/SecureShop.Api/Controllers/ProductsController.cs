using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureShop.Application.DTOs;
using SecureShop.Domain.Entities;
using SecureShop.Infrastructure.Data;

namespace SecureShop.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // Público: Cualquiera puede ver productos
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.Stock, p.CategoryId, p.Category.Name))
            .ToListAsync();
        return Ok(products);
    }

    // Protegido: Solo ADMIN puede crear productos
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Producto creado con éxito", ProductId = product.Id });
    }
}