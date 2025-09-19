using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using VerticalSliceArchitecture.Infrastructure;
using VerticalSliceArchitecture.Infrastructure.Auth;

namespace VerticalSliceArchitecture.Features.Auth
{
    public static class Login
    {
        // In
        public sealed record Request(string UserName, string Password);

        // Out
        public sealed record Response(string access_token, string token_type, int expires_in);

        // Business logic burada
        public sealed class Handler
        {
            private readonly AppDbContext _db;
            private readonly IPasswordHasher _hasher;
            private readonly IJwtTokenService _jwt;
            private readonly AuthOptions _opts;

            public Handler(AppDbContext db, IPasswordHasher hasher, IJwtTokenService jwt, AuthOptions opts)
            {
                _db = db;
                _hasher = hasher;
                _jwt = jwt;
                _opts = opts;
            }

            public async Task<IResult> Handle(Request req, CancellationToken ct)
            {
                if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Password))
                    return TypedResults.BadRequest(new { error = "Username and password are required." });

                var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == req.UserName, ct);
                if (user is null || !_hasher.Verify(user.PasswordHash, req.Password))
                    return TypedResults.Unauthorized();

                var token = _jwt.CreateAccessToken(user);
                return TypedResults.Ok(new Response(
                    access_token: token,
                    token_type: "Bearer",
                    expires_in: _opts.AccessTokenMinutes * 60
                ));
            }
        }

        // Map sadece endpoint’i tanımlar, işi Handler yapar
        public static void Map(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth").WithTags("Auth");

            group.MapPost("/login", async (Request req, Handler handler, CancellationToken ct)
                => await handler.Handle(req, ct));
        }
    }
}
