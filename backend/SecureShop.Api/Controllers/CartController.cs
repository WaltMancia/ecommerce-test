using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureShop.Application.DTOs;
using SecureShop.Application.Services;

namespace SecureShop.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(
        ICartService cartService,
        ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
        {
            throw new UnauthorizedAccessException(
                "No se pudo identificar al usuario autenticado."
            );
        }

        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyCart()
    {
        var userId = GetCurrentUserId();

        var cart = await _cartService.GetCartAsync(userId);

        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItemToCart(
        [FromBody] AddToCartRequest request)
    {
        var userId = GetCurrentUserId();

        try
        {
            await _cartService.AddItemAsync(userId, request);

            return Ok(new
            {
                Message = "Producto agregado al carrito."
            });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al agregar un producto al carrito del usuario {UserId}.",
                userId
            );

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { Error = "Ocurrió un error interno al actualizar el carrito." }
            );
        }
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        if (productId <= 0)
        {
            return BadRequest(new
            {
                Error = "El identificador del producto no es válido."
            });
        }

        var userId = GetCurrentUserId();

        try
        {
            await _cartService.RemoveItemAsync(userId, productId);

            return Ok(new
            {
                Message = "Producto eliminado del carrito."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al eliminar el producto {ProductId} del carrito del usuario {UserId}.",
                productId,
                userId
            );

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { Error = "Ocurrió un error interno al actualizar el carrito." }
            );
        }
    }
}