using System.Security.Claims;
using HomeSeller.CartService.DTOs.Cart;
using HomeSeller.CartService.Interfaces;
using HomeSeller.CartService.Module;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HomeSeller.CartService.Controllers;

[Route("api/cart")]
[ApiController]
[Authorize]
[EnableRateLimiting("sliding_window")]
public class CartController : ControllerBase
{
    public readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpPost("AddCart")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> AddCart([FromBody] CartRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetUserId();
            var result = _cartService.AddItemAsync(userId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [HttpGet("GetCart")]
    [ProducesResponseType(200)]
    public async Task<ActionResult> GetCartByUserId()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetUserId();
            var result = await _cartService.GetCartByUserIdAsync(userId);
            return result == null ? NotFound() : Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("cartitems/{productId:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteCart([FromRoute] Guid productId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetUserId();
            var result = await _cartService.RemoveItemAsync(userId, productId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("clearCart")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> ClearCart()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetUserId();
            await _cartService.ClearCartAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private Guid GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value
                      ?? User.FindFirst("id")?.Value;

        if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Идентификатор пользователя не найден в токене.");
        }

        return userId;
    }
}