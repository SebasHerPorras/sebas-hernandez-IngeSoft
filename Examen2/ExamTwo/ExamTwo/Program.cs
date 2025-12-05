using ExamTwo.Data;
using ExamTwo.Interfaces;
using ExamTwo.Repositories;
using ExamTwo.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Coffee Machine API",
        Version = "v1",
        Description = "API for managing coffee machine operations with SOLID principles and Repository Pattern"
    });
});

// Register Database as Singleton (in-memory data store)
builder.Services.AddSingleton<Database>();

// Register Repository (Scoped - one per request)
builder.Services.AddScoped<ICoffeeMachineRepository, CoffeeMachineRepository>();

// Register Services (Scoped - one per request)
builder.Services.AddScoped<ICoffeeOrderService, CoffeeOrderService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IChangeCalculatorService, ChangeCalculatorService>();

// Register Strategy (Transient - new instance each time)
builder.Services.AddTransient<IChangeStrategy, GreedyChangeService>();

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

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
