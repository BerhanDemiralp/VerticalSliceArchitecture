using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Data.Common;
using VerticalSliceArchitecture.Common;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.Categories;
using VerticalSliceArchitecture.Features.Category;
using VerticalSliceArchitecture.Features.FeatureFlags;
using VerticalSliceArchitecture.Features.Products;
using VerticalSliceArchitecture.Infrastructure;
using VerticalSliceArchitecture.Services.Caching;
using VerticalSliceArchitecture.Services.FeatureFlag;

var builder = WebApplication.CreateBuilder(args);

// CORS HTML frontend 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.WithOrigins("http://localhost:8000")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Redis
var redisCfg = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    // Prod’da bunu: builder.Configuration.GetConnectionString("Redis") vs. yapýn
    options.Configuration = redisCfg;
    options.InstanceName = "vsa:"; // key prefix
});
builder.Services.AddSingleton(typeof(ICacheService<>), typeof(CacheService<>));
builder.Services.AddScoped<IProductCache, ProductCacheService>();
builder.Services.AddScoped<ICategoryCache, CategoryCacheService>();
builder.Services.AddScoped<IFeatureFlagService, FeatureFlagService>();
builder.Services.AddScoped<IFeatureFlagCache, FeatureFlagCacheService>();
// Feature Flag Handlers
builder.Services.AddScoped<UpdateFlagStatus.Handler>();
builder.Services.AddScoped<GetAllFeatureFlags.Handler>();



// Product Handlers
builder.Services.AddScoped<GetAllProducts.Handler>();
builder.Services.AddScoped<GetProductById.Handler>();
builder.Services.AddScoped<CreateProduct.Handler>();
builder.Services.AddScoped<DeleteProduct.Handler>();
builder.Services.AddScoped<UpdateProduct.Handler>();
// Category Handlers
builder.Services.AddScoped<GetAllCategories.Handler>();
builder.Services.AddScoped<GetCategoryById.Handler>();
builder.Services.AddScoped<CreateCategory.Handler>();
builder.Services.AddScoped<DeleteCategory.Handler>();
builder.Services.AddScoped<UpdateCategory.Handler>();

var dbConn = builder.Configuration["ConnectionStrings:Connection"]
    ?? "Data Source=/data/VS.db;Cache=Shared;Mode=ReadWriteCreate;Pooling=True;Journal Mode=Delete;Foreign Keys=True";

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(dbConn));
// builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowFrontend");

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapEndpoints();

// Veritabaný migrate/ensure
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInit");
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Database migration is correct
    await db.Database.MigrateAsync();
    logger.LogInformation("Database Updated.");

    FeatureFlagInitializer.SeedFeatureFlags(db);
    logger.LogInformation("Initial data added.");
}



app.Run();
