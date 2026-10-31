using System.Security.Claims;
using HomeSellerServer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProductService.DTO.Products;

namespace ProductService.Controllers;

[Route("api/Product")]
[ApiController]
[Authorize]
[EnableRateLimiting("sliding_window")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("GetProduct/{productId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductsResponse), 200)]
    public async Task<IActionResult> GetByIdProduct([FromRoute] Guid productId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _productService.GetProductByIdAsync(productId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetProducts")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductsResponse), 200)]
    public async Task<IActionResult> GetAllProducts([FromQuery] string? search = null)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _productService.GetProductsAsync(search);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("AddProduct")]
    [ProducesResponseType(typeof(ProductsResponse), 200)]
    public async Task<IActionResult> AddProduct([FromBody] ProductsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetUserId();
            var result = await _productService.CreateProductAsync(userId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("UpdateProduct/{productId:guid}")]
    [ProducesResponseType(typeof(ProductsResponse), 200)]
    public async Task<IActionResult> UpdateProduct([FromRoute] Guid productId, [FromBody] ProductsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetUserId();
            var result = await _productService.UpdateProductsAsync(userId, productId, request);
            return result == null ? NotFound() : Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpDelete("DeleteProduct/{productId:guid}")]
    [ProducesResponseType(typeof(ProductsResponse), 200)]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid productId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetUserId();
            var result = await _productService.DeleteProductsAsync(userId, productId);
            return Ok(result);
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