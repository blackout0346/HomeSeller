using System.Security.Claims;
using HomeSeller.CartService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeSeller.CartService.Controllers;
[Route("api/cart")]
[ApiController]
[Authorize]
public class CartController: ControllerBase
{
    public readonly ICartService _cartService;
    public CartController(ICartService cartService)
    {
        _cartService = cartService;
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
            var result =await _cartService.GetCartByUserIdAsync(userId);
            return Ok(result);            
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
            var result =await _cartService.RemoveItemAsync(userId, productId);
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
    public async Task<ActionResult> ClearCart( )
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