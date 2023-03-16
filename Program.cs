var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

app.MapGet("/", () => "{'msg':'hellow there','status':200}");
app.MapControllers();
app.Run();
