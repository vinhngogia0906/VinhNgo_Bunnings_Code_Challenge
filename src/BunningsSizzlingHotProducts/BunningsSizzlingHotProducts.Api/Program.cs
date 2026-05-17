using BunningsSizzlingHotProducts.Application;
using BunningsSizzlingHotProducts.Infrastructure;
using BunningsSizzlingHotProducts.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddCors(o => o.AddDefaultPolicy(policy => 
    policy.WithOrigins("http://localhost:5173")
    .AllowAnyMethod()
    .AllowAnyHeader()
 ));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

var inputsDir = app.Configuration.GetValue<string>("Seeding:InputsPath")
                  ?? Path.Combine(app.Environment.ContentRootPath, "inputs");
// Configure the HTTP request pipeline.
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync(
        ordersJsonPath: Path.Combine(inputsDir, "orders.json"),
        productsJsonPath: Path.Combine(inputsDir, "products.json"),
        ct: CancellationToken.None);
}

app.UseCors();
app.UseExceptionHandler(_ => { });
app.MapOpenApi();
app.MapControllers();

app.Run();

public partial class Program { }
