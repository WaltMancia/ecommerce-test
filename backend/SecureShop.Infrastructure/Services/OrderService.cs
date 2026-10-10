using Microsoft.EntityFrameworkCore;
using SecureShop.Application.Services;
using SecureShop.Domain.Entities;
using SecureShop.Infrastructure.Data;

namespace SecureShop.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly PaymentGatewayClient _paymentClient;
    private readonly NotificationClient _notificationClient;

    public OrderService(AppDbContext context, PaymentGatewayClient paymentClient, NotificationClient notificationClient)
    {
        _context = context;
        _paymentClient = paymentClient;
        _notificationClient = notificationClient;
    }

    public async Task<string> CheckoutAsync(int userId)
    {
        // 1. Obtener carrito y usuario
        var cart = await _context.Carts.Include(c => c.Items).ThenInclude(i => i.Product)
                                       .FirstOrDefaultAsync(c => c.UserId == userId);
        var user = await _context.Users.FindAsync(userId);

        if (cart == null || !cart.Items.Any()) throw new Exception("El carrito está vacío.");

        // 2. Validación crítica de Stock y recálculo de precios
        decimal totalAmount = 0;
        foreach (var item in cart.Items)
        {
            if (item.Product.Stock < item.Quantity)
                throw new Exception($"Stock insuficiente para {item.Product.Name}");
            totalAmount += (item.Product.Price * item.Quantity);
        }

        // 3. Crear pedido en estado "Pending"
        var order = new Order
        {
            UserId = userId,
            TotalAmount = totalAmount,
            Status = "Pending",
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.Product.Price
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(); // Genera el OrderId

        // 4. Llamada al sistema externo (Payment Gateway)
        var paymentResult = await _paymentClient.ProcessPaymentAsync(order.Id, order.TotalAmount);

        if (!paymentResult.Success)
        {
            order.Status = "Failed";
            await _context.SaveChangesAsync();
            throw new Exception("El pago fue rechazado.");
        }

        // 5. Consolidar compra exitosa
        order.Status = "Paid";
        order.PaymentReference = paymentResult.TransactionId;

        // Descontar stock (operación atómica en BD)
        foreach (var item in cart.Items)
        {
            item.Product.Stock -= item.Quantity;
        }

        // Limpiar carrito
        _context.CartItems.RemoveRange(cart.Items);
        await _context.SaveChangesAsync();

        // 6. Notificación asíncrona
        _ = _notificationClient.SendEmailAsync(user!.Email, "Confirmación de Compra", $"Tu pedido {order.Id} fue pagado con éxito.");

        return paymentResult.TransactionId;
    }
}