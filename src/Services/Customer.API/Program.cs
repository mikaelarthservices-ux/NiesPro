using Customer.API.Extensions;
using NiesPro.Contracts.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Customer API", Version = "v1" });
    c.EnableAnnotations();
});

// Add Customer services
builder.Services.AddCustomerServices(builder.Configuration);

// Add Customer infrastructure
builder.Services.AddCustomerInfrastructure(builder.Configuration);

// Add Authentication & Authorization
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.RequireHttpsMetadata = false;
        options.Audience = builder.Configuration["Auth:Audience"];
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Always enable Swagger for API documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer API v1");
    c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Configure Customer application
app.ConfigureCustomerApp();

// Ensure database
await app.EnsureDatabaseAsync();

// Start the application
app.Run();