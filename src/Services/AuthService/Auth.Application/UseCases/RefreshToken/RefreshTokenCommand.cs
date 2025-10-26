using Auth.Application.DTOs;
using MediatR;

namespace Auth.Application.UseCases.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResponse>;
