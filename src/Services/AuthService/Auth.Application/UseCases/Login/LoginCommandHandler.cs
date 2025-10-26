using Auth.Application.DTOs;
using Auth.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auth.Application.UseCases.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Find user by username
        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Login failed for username: {Username} - User not found or inactive", request.Username);
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // 2. Verify password (TODO: Replace with BCrypt.Net.Verify in production)
        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for username: {Username} - Invalid password", request.Username);
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // 3. Update last login and prepare response (Token generation will be in API layer)
        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Username} logged in successfully with roles: {Roles}", 
            user.Username, string.Join(", ", user.Roles));

        // Return user data - Token generation will happen in API layer via TokenService
        return new LoginResponse(
            AccessToken: string.Empty, // Will be filled in API layer
            RefreshToken: string.Empty, // Will be filled in API layer
            UserId: user.UserId.ToString(),
            Username: user.Username,
            Email: user.Email,
            FullName: user.FullName,
            Roles: user.Roles,
            ExpiresAt: DateTime.UtcNow.AddHours(1)
        );
    }

    // TODO: Replace with BCrypt.Net.Verify(password, passwordHash) in production
    private bool VerifyPassword(string password, string passwordHash)
    {
        // For demo purposes, we'll use simple comparison
        // In production: return BCrypt.Net.Verify(password, passwordHash);
        return password == passwordHash;
    }
}
