using System.Security.Authentication;
using HomeSellerServer.DTO;
using HomeSellerServer.Module;
using Microsoft.AspNetCore.Mvc;
namespace HomeSellerServer.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), 200)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var result = await _authService.RegisterAsync(request);
            Console.WriteLine($"register result: {result.Token}{result.Login}");
            return Ok(result); 
        }
        catch (AuthenticationException ex)
        {
            return BadRequest(ex);
        }
    }
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var result = await _authService.LoginAsync(request);
            Console.WriteLine($"login result: {result.Token}{result.Login}");
            return Ok(result); 
        }
        catch (AuthenticationException)
        {
      
            return Unauthorized(new { message = "Invalid login or password." });
        }
    }
    [HttpPost("ChangePassword")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            await _authService.ChangePasswordAsync(request);
            return Ok(new { message = "Password updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("DeleteUser/{userId}")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    public async Task<IActionResult> DeleteUser([FromBody] Guid userId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _authService.DeleteUserAsync(userId);
            return Ok(new { message = "User deleted successfully." });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    
        
    }
}