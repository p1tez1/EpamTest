using Microsoft.EntityFrameworkCore;
using Northwind.Orders.WebApi.Services;
using Northwind.Services.EntityFramework.Entities;
using Northwind.Services.EntityFramework.Repositories;
using Northwind.Services.Repositories;

var builder = WebApplication.CreateBuilder(args);

var sqliteDatabaseFile = builder.Configuration.GetValue<string?>("SQLiteDatabaseFile", null) ?? throw new Exception("The `SQLiteDatabaseFile` configuration setting is not set.");
var connectionString = builder.Configuration.GetConnectionString("NorthwindDatabase") ?? throw new Exception("The `NorthwindDatabase` connection string is not set.");

if (File.Exists(sqliteDatabaseFile))
{
    File.Delete(sqliteDatabaseFile);
}

using var databaseService = new DatabaseService(connectionString!);
databaseService.InitializeDatabase();

// Add services to the container.
builder.Services.AddDbContext<NorthwindContext>(options => options.UseSqlite(connectionString));
builder.Services.AddControllers();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
