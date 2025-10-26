using EMIS.Authentication.Models;

namespace EMIS.Authentication.Services;

public interface ITokenService
{
    string GenerateAccessToken(AuthUser user);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
}
