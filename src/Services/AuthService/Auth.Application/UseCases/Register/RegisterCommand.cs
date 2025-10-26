using Auth.Application.DTOs;
using MediatR;

namespace Auth.Application.UseCases.Register;

public record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string FullName,
    string PhoneNumber,
    List<string> Roles
) : IRequest<RegisterResponse>;
