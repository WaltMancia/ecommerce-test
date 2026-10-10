using SecureShop.Application.DTOs;

namespace SecureShop.Application.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);
    Task AddItemAsync(int userId, AddToCartRequest request);
    Task RemoveItemAsync(int userId, int productId);
}