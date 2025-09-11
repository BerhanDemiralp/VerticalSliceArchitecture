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
var redisCfg = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    // Prod’da bunu: builder.Configuration.GetConnectionString("Redis") vs. yapýn
    options.Configuration = redisCfg;
    options.InstanceName = "vsa:"; // key prefix
});

var dbConn = builder.Configuration["ConnectionStrings:Connection"] ?? builder.Configuration.GetConnectionString("Connection");
builder.Services.AddDbContext<AppDbContext>(option => option.UseSqlite(dbConn));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

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

// Minimal API Endpoints
app.MapEndpoints();

app.Run();
