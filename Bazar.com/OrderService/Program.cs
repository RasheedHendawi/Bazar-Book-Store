
using OrderServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<OrderService>();

var app = builder.Build();
app.MapControllers();
app.Run();
