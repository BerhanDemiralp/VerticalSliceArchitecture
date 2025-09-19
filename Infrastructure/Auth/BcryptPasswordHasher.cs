using BCrypt.Net;

namespace VerticalSliceArchitecture.Infrastructure.Auth;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string hash, string password) => BCrypt.Net.BCrypt.Verify(password, hash);
}
