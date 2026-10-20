using HomeSellerServer.DTO.Products;
using HomeSellerServer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeSellerServer.Controllers;

[Route("api/Product")]
[ApiController]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }
    [HttpGet("GetProduct{productId:guid}")]
    [ProducesResponseType(typeof(ProductsResponse), 200)]
    public async Task<IActionResult> GetByIdProduct(Guid productId)
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
    [ProducesResponseType(typeof(ProductsResponse), 200)]
    public async Task<IActionResult> GetAllProducts(  [FromQuery]  string? search  = null)
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
            var result = await _productService.CreateProductAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }
    [HttpPut("UpdateProduct{productId:guid}")]
    [ProducesResponseType(typeof(ProductsResponse), 200)]
    public async Task<IActionResult> UpdateProduct(Guid productId,[FromBody] ProductsRequest request )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _productService.UpdateProductsAsync(productId,request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }
    [HttpDelete("DeleteProduct/{productId:guid}")]
    [ProducesResponseType(typeof(ProductsResponse), 200)]
    public async Task<IActionResult> DeleteProduct(Guid productId )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _productService.DeleteProductsAsync(productId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }
}