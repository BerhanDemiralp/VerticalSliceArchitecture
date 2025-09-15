using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Data.Common;
using VerticalSliceArchitecture.Common;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.Categories;
using VerticalSliceArchitecture.Features.Category;
using VerticalSliceArchitecture.Features.Products;
using VerticalSliceArchitecture.Infrastructure;
using VerticalSliceArchitecture.Services.Caching;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Redis
var redisCfg = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    // Prod�da bunu: builder.Configuration.GetConnectionString("Redis") vs. yap�n
    options.Configuration = redisCfg;
    options.InstanceName = "vsa:"; // key prefix
});
builder.Services.AddSingleton(typeof(ICacheService<>), typeof(CacheService<>));
builder.Services.AddScoped<IProductCache, ProductCacheService>();
builder.Services.AddScoped<ICategoryCache, CategoryCacheService>();

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

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapEndpoints();

// Veritaban� migrate/ensure
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInit");
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Sadece migrate (silme yok!)
    await db.Database.MigrateAsync();

    // Ba�land���n ger�ek dosya yolu:
    DbConnection cnn = db.Database.GetDbConnection();
}

app.Run();
