using Auth.Application.DTOs;
using Auth.Application.UseCases.Login;
using Auth.Application.UseCases.RefreshToken;
using Auth.Application.UseCases.Register;
using Auth.Domain.Repositories;
using EMIS.Authentication.Models;
using EMIS.Authentication.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.API.Controllers;

/// <summary>
/// Centralized Authentication API
/// This service handles all authentication and authorization for the EMIS system.
/// All user types (Teachers, Parents, Admins) login through this service.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Login endpoint for all user types
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT access token and refresh token</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<Auth.Application.DTOs.LoginResponse>> Login([FromBody] Auth.Application.DTOs.LoginRequest request)
    {
        try
        {
            var command = new LoginCommand(request.Username, request.Password);
            var userResponse = await _mediator.Send(command);

            // Generate tokens
            var authUser = new AuthUser
            {
                UserId = Guid.Parse(userResponse.UserId),
                Username = userResponse.Username,
                Email = userResponse.Email,
                FullName = userResponse.FullName,
                Roles = userResponse.Roles
            };

            var accessToken = _tokenService.GenerateAccessToken(authUser);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Update refresh token in database
            var user = await _unitOfWork.Users.GetByUsernameAsync(userResponse.Username);
            if (user != null)
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }

            var response = userResponse with
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            
            _logger.LogInformation("User {Username} logged in successfully", request.Username);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for username: {Username}", request.Username);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Register a new user (Admin, Teacher, or Parent)
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Created user information</returns>
    [HttpPost("register")]
    [AllowAnonymous] // TODO: In production, restrict to Admin role only
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterCommand(
                request.Username,
                request.Email,
                request.Password,
                request.FullName,
                request.PhoneNumber,
                request.Roles
            );

            var response = await _mediator.Send(command);
            
            _logger.LogInformation("New user registered: {Username}", request.Username);
            return CreatedAtAction(nameof(GetMe), new { }, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for username: {Username}", request.Username);
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>New access token and refresh token</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<Auth.Application.DTOs.RefreshTokenResponse>> RefreshToken([FromBody] Auth.Application.DTOs.RefreshTokenRequest request)
    {
        try
        {
            var command = new RefreshTokenCommand(request.RefreshToken);
            var tokenResponse = await _mediator.Send(command);

            // Get user by refresh token to generate new tokens
            var user = await _unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            // Generate new tokens
            var authUser = new AuthUser
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Roles = user.Roles
            };

            var newAccessToken = _tokenService.GenerateAccessToken(authUser);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Update refresh token in database
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var response = tokenResponse with
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
            
            _logger.LogInformation("Tokens refreshed successfully");
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Refresh token failed: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }

    /// <summary>
    /// Get current authenticated user information from JWT token
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var fullName = User.FindFirst("FullName")?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            userId,
            username,
            email,
            fullName,
            roles
        });
    }
}
