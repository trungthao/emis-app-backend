using Auth.Application.DTOs;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auth.Application.UseCases.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RegisterCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate username uniqueness
        if (await _unitOfWork.Users.UsernameExistsAsync(request.Username))
        {
            _logger.LogWarning("Registration failed - Username already exists: {Username}", request.Username);
            throw new InvalidOperationException($"Username '{request.Username}' is already taken");
        }

        // 2. Validate email uniqueness
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
        {
            _logger.LogWarning("Registration failed - Email already exists: {Email}", request.Email);
            throw new InvalidOperationException($"Email '{request.Email}' is already registered");
        }

        // 3. Create new user
        // TODO: Replace with BCrypt.Net.HashPassword(request.Password) in production
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Roles = request.Roles ?? new List<string> { "Parent" }, // Default role
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User registered successfully: {Username} with roles: {Roles}", 
            user.Username, string.Join(", ", user.Roles));

        return new RegisterResponse(
            UserId: user.UserId.ToString(),
            Username: user.Username,
            Email: user.Email,
            Message: "User registered successfully"
        );
    }

    // TODO: Replace with BCrypt.Net.HashPassword in production
    private string HashPassword(string password)
    {
        // For demo purposes, we'll store plain text
        // In production: return BCrypt.Net.HashPassword(password);
        return password;
    }
}
