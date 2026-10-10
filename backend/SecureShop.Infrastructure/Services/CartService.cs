using Microsoft.EntityFrameworkCore;
using SecureShop.Application.DTOs;
using SecureShop.Application.Services;
using SecureShop.Domain.Entities;
using SecureShop.Infrastructure.Data;

namespace SecureShop.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _context;

    public CartService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await _context.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            return new CartDto(0, 0, new List<CartItemDto>());
        }

        var itemsDto = cart.Items.Select(i => new CartItemDto(
            i.ProductId,
            i.Product.Name,
            i.Product.Price,
            i.Quantity,
            i.Quantity * i.Product.Price
        )).ToList();

        var total = itemsDto.Sum(i => i.Subtotal);

        return new CartDto(cart.Id, total, itemsDto);
    }

    public async Task AddItemAsync(int userId, AddToCartRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Quantity),
                "La cantidad debe ser mayor a 0."
            );
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId);

        if (product is null || !product.IsActive)
        {
            throw new InvalidOperationException("Producto no disponible.");
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        var existingItem = cart?.Items
            .FirstOrDefault(i => i.ProductId == request.ProductId);

        var currentQuantity = existingItem?.Quantity ?? 0;
        var newQuantity = currentQuantity + request.Quantity;

        if (product.Stock < newQuantity)
        {
            throw new InvalidOperationException("Stock insuficiente para la cantidad solicitada.");
        }

        if (cart is null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
        }

        if (existingItem is not null)
        {
            existingItem.Quantity = newQuantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(int userId, int productId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            return;
        }

        var item = cart.Items
            .FirstOrDefault(i => i.ProductId == productId);

        if (item is null)
        {
            return;
        }

        cart.Items.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}