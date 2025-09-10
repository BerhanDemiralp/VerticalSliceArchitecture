using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.Products;
using VerticalSliceArchitecture.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
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

// Minimal API endpoint definitions for Products
app.MapGet("/api/products", async (AppDbContext context) =>
{
    var products = await context.Products.ToListAsync();

    var response = products.Select(p => new GetAllProducts.Response(
        p.Id,
        p.Name,
        p.Price,
        p.CategoryId
    ));

    return Results.Ok(response);
})
.WithName("GetAllProducts")
.WithOpenApi();

app.MapPost("/api/products", async (CreateProduct.Request request, AppDbContext context) =>
{
    var product = new Product
    {
        Name = request.Name,
        Price = request.Price,
        CategoryId = request.CategoryId
    };

    context.Products.Add(product);
    await context.SaveChangesAsync();

    var response = new CreateProduct.Response(product.Id, product.Name, product.Price, product.CategoryId);

    // HTTP 201 Created yanýtý döndürmek en iyi yaklaþýmdýr.
    // Bu, istemciye yeni oluþturulan kaynaðýn URL'ini verir.
    return Results.Created($"/api/products/{response.Id}", response);
})
.WithName("CreateProduct")
.WithOpenApi();

app.MapPut("/api/products/{id}", async (int id, UpdateProduct.Request request, AppDbContext context) =>
{
    var product = await context.Products.FindAsync(id);
    if (product == null)
    {
        return Results.NotFound();
    }
    product.Name = request.Name;
    product.Price = request.Price;
    product.CategoryId = request.CategoryId;
    await context.SaveChangesAsync();
    var response = new UpdateProduct.Response(product.Id, product.Name, product.Price, product.CategoryId);
    return Results.Ok(response);
})
.WithName("UpdateProduct")
.WithOpenApi();


app.Run();
