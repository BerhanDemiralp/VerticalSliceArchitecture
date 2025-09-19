namespace VerticalSliceArchitecture.Infrastructure.Auth;

public class AuthOptions
{
    public string Issuer { get; set; } = "vsa";
    public string Audience { get; set; } = "vsa.clients";
    public string Secret { get; set; } = default!; 
    public int AccessTokenMinutes { get; set; } = 30;
    public int RefreshTokenDays { get; set; } = 7;
}
