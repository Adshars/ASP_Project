using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var warehouseConnectionString = builder.Configuration.GetConnectionString("WarehouseDB");
builder.Services.AddDbContext<WarehouseDbContext>((options) =>
{
    options.UseSqlServer(warehouseConnectionString);
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
