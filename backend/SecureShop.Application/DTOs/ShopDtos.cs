namespace SecureShop.Application.DTOs;

public record CategoryDto(int Id, string Name, string Description);
public record CreateCategoryRequest(string Name, string Description);

public record ProductDto(int Id, string Name, string Description, decimal Price, int Stock, int CategoryId, string CategoryName);
public record CreateProductRequest(string Name, string Description, decimal Price, int Stock, int CategoryId);

public record CartItemDto(int ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal Subtotal);
public record CartDto(int CartId, decimal TotalAmount, List<CartItemDto> Items);
public record AddToCartRequest(int ProductId, int Quantity);