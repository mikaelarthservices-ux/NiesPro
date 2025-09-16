using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Microsoft.OpenApi.Models;
using Gateway.API.Services;
using Gateway.API.Middleware;

// Configuration Serilog pour logging professionnel
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Ajouter Serilog
builder.Host.UseSerilog();

// Configuration JWT pour authentification centralisée
var jwtKey = builder.Configuration["JwtSettings:Key"] ?? "NiesPro_Gateway_Secret_Key_2024_Professional_Architecture";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "NiesPro.Gateway";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "NiesPro.APIs";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Configuration CORS professionnelle
builder.Services.AddCors(options =>
{
    options.AddPolicy("NiesProPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:3000", "https://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Services professionnels
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<ITokenValidationService, TokenValidationService>();
builder.Services.AddHttpClient<HttpProxyMiddleware>();
builder.Services.AddScoped<ITokenValidationService, TokenValidationService>();
builder.Services.AddControllers();

// Configuration Health Checks simplifiée
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Configuration Swagger pour documentation agrégée
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "NiesPro Gateway API", 
        Version = "v1.0",
        Description = "Gateway API professionnel orchestrant Auth.API, Order.API et Catalog.API",
        Contact = new OpenApiContact
        {
            Name = "NiesPro Team",
            Email = "contact@niespro.com"
        }
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configuration pipeline middleware professionnel
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NiesPro Gateway API V1");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
});

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("NiesProPolicy");

// Middlewares personnalisés professionnels
app.UseMiddleware<JwtAuthenticationMiddleware>();
app.UseMiddleware<HttpProxyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Health Checks endpoints
app.MapHealthChecks("/health");

// Map controllers
app.MapControllers();

// Logging des requêtes pour monitoring
app.Use(async (context, next) =>
{
    var stopWatch = System.Diagnostics.Stopwatch.StartNew();
    await next();
    stopWatch.Stop();
    
    Log.Information("Gateway Request: {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms", 
        context.Request.Method, 
        context.Request.Path, 
        context.Response.StatusCode, 
        stopWatch.ElapsedMilliseconds);
});

app.Run();