using Auth.Application.DTOs;
using Auth.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auth.Application.UseCases.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Find user by refresh token
        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Refresh token validation failed - Invalid token or inactive user");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // 2. Validate refresh token expiry
        if (user.RefreshTokenExpiry == null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token expired for user: {Username}", user.Username);
            throw new UnauthorizedAccessException("Refresh token has expired");
        }

        _logger.LogInformation("Tokens refreshed successfully for user: {Username}", user.Username);

        // Return user data - Token generation will happen in API layer
        return new RefreshTokenResponse(
            AccessToken: string.Empty, // Will be filled in API layer
            RefreshToken: string.Empty, // Will be filled in API layer
            ExpiresAt: DateTime.UtcNow.AddHours(1)
        );
    }
}
