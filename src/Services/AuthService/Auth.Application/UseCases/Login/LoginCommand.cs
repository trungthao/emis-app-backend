using Auth.Application.DTOs;
using MediatR;

namespace Auth.Application.UseCases.Login;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponse>;
