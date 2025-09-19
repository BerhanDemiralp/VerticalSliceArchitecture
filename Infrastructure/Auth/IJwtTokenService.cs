using System.Security.Claims;
using System.Text;
using VerticalSliceArchitecture.Domain;

namespace VerticalSliceArchitecture.Infrastructure.Auth;

public interface IJwtTokenService
{
    string CreateAccessToken(User user);
}
