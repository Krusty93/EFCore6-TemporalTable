using System.ComponentModel.DataAnnotations;
using System.Reflection;
using EFCore6.TemporalTables.API;
using EFCore6.TemporalTables.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<TemporalDbContext>(b =>
{
    var cs = builder.Configuration.GetConnectionString("SqlServer");
    b.UseSqlServer(cs);

    b.ConfigureLoggingCacheTime(TimeSpan.FromDays(1));
    b.EnableThreadSafetyChecks(false);
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opt =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    opt.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(opt =>
{
    opt.DisplayRequestDuration();
    opt.SwaggerEndpoint("/swagger/v1/swagger.json", $"{builder.Environment.ApplicationName} v1");
});

app.MapGet("product/{id:guid}", async (Guid id, IProductRepository repository) =>
{
    return await repository.GetAsync(id);
});

app.MapGet("product/{id:guid}/history", async (Guid id, [Required, FromQuery] DateTime time, IProductRepository repository) =>
{
    return await repository.GetTemporalAsync(id, time);
});

app.MapGet("product/{id:guid}/history/all", async (Guid id, IProductRepository repository) =>
{
    return await repository.GetTemporalAsync(id);
});

app.MapGet("product/{id:guid}/history-range", async (Guid id, [Required, FromQuery] DateTime from, [Required, FromQuery] DateTime to, IProductRepository repository) =>
{
    return await repository.GetTemporalAsync(id, from, to);
});

app.MapPost("product/{id:guid}/rollback", async (Guid id, [Required, FromQuery] DateTime time, IProductRepository repository) =>
{
    return await repository.RollbackTemporalDataAsync(id, time);
});

app.MapPost("product/{id:guid}/restore", async (Guid id, IProductRepository repository) =>
{
    return await repository.RestoreDeletedProductAsync(id);
});

app.MapPost("product", async (Product product, IProductRepository repository) =>
{
    var result = await repository.CreateAsync(product);
    return result;
});

app.MapDelete("product/{id:guid}", async (Guid id, IProductRepository repository) =>
{
    await repository.DeleteAsync(id);
    return Results.NoContent();
});

app.MapPut("product", async (Product product, IProductRepository repository) =>
{
    await repository.UpdateAsync(product);
    return product;
});

app.Run();
