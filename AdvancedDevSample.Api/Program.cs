using AdvancedDevSample.Api.Middlewares;
using AdvancedDevSample.Application.Services;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Documentation XML pour Swagger
builder.Services.AddSwaggerGen(options =>
{
    var basePath = AppContext.BaseDirectory;
    foreach (var xmlFile in Directory.GetFiles(basePath, "*.xml"))
    {
        options.IncludeXmlComments(xmlFile);
    }
});

// ===== Stockage en mémoire (Singleton pour persister tant que l'app tourne) =====
builder.Services.AddSingleton<InMemoryDataStore>();

// ===== Dépendances Application =====
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<OrderService>();

// ===== Dépendances Infrastructure =====
builder.Services.AddScoped<IProductRepository, InMemoryProductRepository>();
builder.Services.AddScoped<ICustomerRepository, InMemoryCustomerRepository>();
builder.Services.AddScoped<ISupplierRepository, InMemorySupplierRepository>();
builder.Services.AddScoped<IOrderRepository, InMemoryOrderRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();

// Rendre la classe Program accessible aux tests d'intégration
public partial class Program { }
