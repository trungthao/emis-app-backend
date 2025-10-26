namespace Auth.Application.DTOs;

public record LoginRequest(string Username, string Password);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    string UserId,
    string Username,
    string Email,
    string FullName,
    List<string> Roles,
    DateTime ExpiresAt
);

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FullName,
    string PhoneNumber,
    List<string> Roles
);

public record RegisterResponse(
    string UserId,
    string Username,
    string Email,
    string Message
);

public record RefreshTokenRequest(string RefreshToken);

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
