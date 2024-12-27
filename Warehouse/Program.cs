using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var warehouseConnectionString = builder.Configuration.GetConnectionString("WarehouseDB");
builder.Services.AddDbContext<WarehouseDbContext>(options => options.UseSqlServer(warehouseConnectionString));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
