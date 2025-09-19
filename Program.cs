using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using VerticalSliceArchitecture.Common;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.Auth;
using VerticalSliceArchitecture.Features.Categories;
using VerticalSliceArchitecture.Features.Category;
using VerticalSliceArchitecture.Features.FeatureFlags;
using VerticalSliceArchitecture.Features.Products;
using VerticalSliceArchitecture.Infrastructure;
using VerticalSliceArchitecture.Infrastructure.Auth;
using VerticalSliceArchitecture.Services.Caching;
using VerticalSliceArchitecture.Services.FeatureFlag;

var builder = WebApplication.CreateBuilder(args);

//
// ===================== Options & Auth DI =====================
var authOptions = new AuthOptions();
builder.Configuration.GetSection("Auth").Bind(authOptions);

// Env ile override edilebilsin:
authOptions.Secret ??= builder.Configuration["Auth:Secret"] ?? "dev__replace_me_with_long_random_secret";

// DI: Auth servisleri
builder.Services.AddSingleton(authOptions);
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// JWT Authentication & Authorization
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidIssuer = authOptions.Issuer,
            ValidAudience = authOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Secret)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

//
// ===================== CORS =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", cors =>
    {
        cors.WithOrigins("http://localhost:8000", "http://127.0.0.1:8000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

//
// ===================== Web / API =====================
builder.Services.AddControllers();
builder.Services.AddOpenApi();          // Scalar/OpenAPI

// (Ýsteðe baðlý static files: wwwroot/login.html gibi dosyalar için güvenli)
builder.Services.AddRouting();

//
// ===================== Redis =====================
var redisCfg = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisCfg;
    options.InstanceName = "vsa:"; // key prefix
});
builder.Services.AddSingleton(typeof(ICacheService<>), typeof(CacheService<>));
builder.Services.AddScoped<IProductCache, ProductCacheService>();
builder.Services.AddScoped<ICategoryCache, CategoryCacheService>();
builder.Services.AddScoped<IFeatureFlagService, FeatureFlagService>();
builder.Services.AddScoped<IFeatureFlagCache, FeatureFlagCacheService>();

//
// ===================== Handlers (DI) =====================
// Feature Flags
builder.Services.AddScoped<UpdateFlagStatus.Handler>();
builder.Services.AddScoped<GetAllFeatureFlags.Handler>();

// Products
builder.Services.AddScoped<GetAllProducts.Handler>();
builder.Services.AddScoped<GetProductById.Handler>();
builder.Services.AddScoped<CreateProduct.Handler>();
builder.Services.AddScoped<CreateProductV2.Handler>();
builder.Services.AddScoped<DeleteProduct.Handler>();
builder.Services.AddScoped<UpdateProduct.Handler>();
builder.Services.AddScoped<UpdateProductV2.Handler>();

// Categories
builder.Services.AddScoped<GetAllCategories.Handler>();
builder.Services.AddScoped<GetCategoryById.Handler>();
builder.Services.AddScoped<CreateCategory.Handler>();
builder.Services.AddScoped<DeleteCategory.Handler>();
builder.Services.AddScoped<UpdateCategory.Handler>();

// Auth
builder.Services.AddScoped<Login.Handler>();

//
// ===================== EF Core =====================
var dbConn = builder.Configuration["ConnectionStrings:Connection"]
    ?? "Data Source=/data/VS.db;Cache=Shared;Mode=ReadWriteCreate;Pooling=True;Journal Mode=Delete;Foreign Keys=True";

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(dbConn));
// builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

//
// ===================== Build =====================
var app = builder.Build();

//
// ===================== Dev-Only Tools =====================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

//
// ===================== Middleware Pipeline =====================
// app.UseHttpsRedirection(); // Ýstersen tekrar açabilirsin
app.UseStaticFiles();          // wwwroot varsa zararsýz; login.html gibi sayfalarý servis eder
app.UseCors("AllowFrontend");

app.UseAuthentication();       // <<< ÖNEMLÝ: Authorization'dan önce olmalý
app.UseAuthorization();

app.MapControllers();
app.MapEndpoints();            // Projedeki extension ile slice endpoint’lerini map eder

//
// ===================== DB Migrate & Seed =====================
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInit");
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    await db.Database.MigrateAsync();
    logger.LogInformation("Database Updated.");

    // Feature flags
    FeatureFlagInitializer.SeedFeatureFlags(db);

    // --- Admin user seed ---
    if (!await db.Users.AnyAsync(u => u.UserName == "admin"))
    {
        db.Users.Add(new VerticalSliceArchitecture.Domain.User
        {
            UserName = "admin",
            PasswordHash = hasher.Hash("Admin@123"),
            Role = "Admin"
        });
        await db.SaveChangesAsync();
        logger.LogInformation("Admin user seeded (admin / Admin@123).");
    }
    else
    {
        logger.LogInformation("Admin user already exists, skipping seed.");
    }

    logger.LogInformation("Initial data added.");
}

app.Run();
