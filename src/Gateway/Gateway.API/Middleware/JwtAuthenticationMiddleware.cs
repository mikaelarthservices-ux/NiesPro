namespace Gateway.API.Middleware;

/// <summary>
/// Middleware d'authentification JWT professionnel pour le Gateway
/// </summary>
public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public JwtAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<JwtAuthenticationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Chemins qui ne nécessitent pas d'authentification
        var publicPaths = new[]
        {
            "/health",
            "/health-ui", 
            "/swagger",
            "/api/auth/login",
            "/api/auth/register",
            "/api/gateway/info",
            "/api/gateway/routes",
            "/api/products" // Catalogue public en lecture
        };

        // Patterns de chemins publics (regex-like)
        var publicPathPatterns = new[]
        {
            "/api/*/health",        // Tous les health checks des services
            "/api/*/swagger",       // Swagger de tous les services
            "/*/health",            // Health checks génériques
            "/*/swagger"            // Swagger génériques
        };

        var path = context.Request.Path.Value?.ToLower() ?? "";
        
        // Vérifier les chemins exacts
        var isPublicPath = publicPaths.Any(p => path.StartsWith(p));
        
        // Vérifier les patterns (health checks des services)
        var isPublicPattern = publicPathPatterns.Any(pattern => 
        {
            var regex = pattern.Replace("*", "[^/]+");
            return System.Text.RegularExpressions.Regex.IsMatch(path, $"^{regex}");
        });

        if (isPublicPath || isPublicPattern)
        {
            _logger.LogDebug("Public path accessed: {Path}", path);
            await _next(context);
            return;
        }

        // Vérifier la présence du token
        var authorizationHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            _logger.LogWarning("Missing or invalid Authorization header for path: {Path}", path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: Missing or invalid token");
            return;
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();

        try
        {
            // Validation basique du format JWT
            if (!IsValidJwtFormat(token))
            {
                _logger.LogWarning("Invalid JWT format for path: {Path}", path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Invalid token format");
                return;
            }

            // Ajouter le token aux headers pour les microservices downstream
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            
            _logger.LogDebug("Authentication successful for path: {Path}", path);
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error for path: {Path}", path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: Authentication failed");
        }
    }

    /// <summary>
    /// Validation basique du format JWT
    /// </summary>
    private bool IsValidJwtFormat(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var parts = token.Split('.');
        return parts.Length == 3; // Header.Payload.Signature
    }
}