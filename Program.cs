using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.Products;
using VerticalSliceArchitecture.Infrastructure;
using VerticalSliceArchitecture.Common;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// using Microsoft.Extensions.Caching.StackExchangeRedis;

builder.Services.AddStackExchangeRedisCache(options =>
{
    // Prod’da bunu: builder.Configuration.GetConnectionString("Redis") vs. yapýn
    options.Configuration = "localhost:6379";
    options.InstanceName = "cache"; // key prefix
});


builder.Services.AddDbContext<AppDbContext>(option => option.UseSqlite(builder.Configuration.GetConnectionString("Connection")));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Minimal API Endpoints
app.MapEndpoints();

app.Run();
