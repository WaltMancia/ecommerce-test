namespace SecureShop.Application.Services;

public interface IOrderService
{
    Task<string> CheckoutAsync(int userId);
}