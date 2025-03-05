using BPN.Payment.API.Data;
using BPN.Payment.API.Services.BalanceManagementService;
using BPN.Payment.API.Services.OrderService;
using BPN.Payment.API.Services.ProductService;
using BPN.Payment.API.Utils.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);


var configuration = builder.Configuration;
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddHttpClient<IBalanceManagementService, BalanceManagementService>();

//Cache
builder.Services.AddMemoryCache(); 

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//gzip
builder.Services.AddResponseCompression();
var app = builder.Build();
app.UseResponseCompression();
app.UseMiddleware<ExceptionHandlingMiddleware>();

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
