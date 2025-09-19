using CustomerService.Application;
using CustomerService.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Reflection;
using FluentValidation.AspNetCore;
using CustomerService.API.Middleware;
using CustomerService.API.Filters;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// CONFIGURATION SERILOG
// ===============================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("ServiceName", "CustomerService")
    .CreateLogger();

builder.Host.UseSerilog();

// ===============================
// SERVICES CONFIGURATION
// ===============================

// Application Services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// API Controllers
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    options.Filters.Add<ExceptionFilter>();
})
.AddFluentValidation();

// API Explorer et Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NiesPro Customer Service API",
        Version = "v1",
        Description = "API du microservice de gestion des clients pour l'application NiesPro",
        Contact = new OpenApiContact
        {
            Name = "NiesPro Dev Team",
            Email = "dev@niespro.com"
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Exemple: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Authentication JWT (intégration avec Auth microservice)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Information("Token validated for user: {UserId}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
    options.AddPolicy("StaffAccess", policy => policy.RequireRole("Staff", "Manager", "Admin"));
});

// CORS pour intégration avec autres microservices
builder.Services.AddCors(options =>
{
    options.AddPolicy("NiesProPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CustomerDbContext>();

// HTTP Client pour communication avec autres microservices
builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Microservices:AuthService:BaseUrl"] ?? "http://localhost:5001");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("LogsService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Microservices:LogsService:BaseUrl"] ?? "http://localhost:5003");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// ===============================
// MIDDLEWARE PIPELINE
// ===============================

// Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Service API V1");
        c.RoutePrefix = string.Empty; // Swagger UI à la racine
    });
}

// Security headers
app.UseMiddleware<SecurityHeadersMiddleware>();

// Request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserId", httpContext.User?.Identity?.Name);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
    };
});

// Exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// CORS
app.UseCors("NiesProPolicy");

// HTTPS Redirection
app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Rate limiting middleware
app.UseMiddleware<RateLimitingMiddleware>();

// Controllers mapping
app.MapControllers();

// Health checks
app.MapHealthChecks("/health");

// Graceful shutdown
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("Customer Service API is shutting down...");
});

Log.Information("Customer Service API starting up...");

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Customer Service API failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}