
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<CatalogServer.Services.BookRepository>();
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();