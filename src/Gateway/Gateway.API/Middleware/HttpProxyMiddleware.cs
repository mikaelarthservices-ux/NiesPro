using System.Net;

namespace Gateway.API.Middleware;

/// <summary>
/// Middleware de routage HTTP professionnel pour proxy vers les microservices
/// </summary>
public class HttpProxyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpProxyMiddleware> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    private readonly Dictionary<string, string> _routingTable = new()
    {
        // Auth Service
        { "/api/auth", "https://localhost:5011" },
        { "/api/users", "https://localhost:5011" },
        { "/api/roles", "https://localhost:5011" },
        
        // Catalog Service  
        { "/api/catalog", "https://localhost:5013" },
        { "/api/products", "https://localhost:5013" },
        { "/api/categories", "https://localhost:5013" },
        { "/api/suppliers", "https://localhost:5013" },
        
        // Order Service
        { "/api/order", "https://localhost:5012" },
        { "/api/orders", "https://localhost:5012" },
        { "/api/invoices", "https://localhost:5012" },
        
        // Payment Service
        { "/api/payment", "https://localhost:5014" },
        { "/api/payments", "https://localhost:5014" },
        { "/api/payment-methods", "https://localhost:5014" },
        
        // Stock Service
        { "/api/stock", "https://localhost:5006" },
        { "/api/warehouses", "https://localhost:5006" },
        { "/api/inventory", "https://localhost:5006" },
        
        // Customer Service (à configurer)
        { "/api/customers", "http://localhost:8001" },
        { "/api/loyalty", "http://localhost:8001" },
        
        // Restaurant Service (à reconstruire)
        { "/api/tables", "https://localhost:7011" },
        { "/api/reservations", "https://localhost:7011" },
        { "/api/menus", "https://localhost:7011" }
    };

    public HttpProxyMiddleware(
        RequestDelegate next,
        ILogger<HttpProxyMiddleware> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        
        // Configuration du HttpClient pour ignorer les erreurs SSL en développement
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        
        // Vérifier si la route doit être proxifiée
        var route = _routingTable.FirstOrDefault(r => path.StartsWith(r.Key, StringComparison.OrdinalIgnoreCase));
        
        if (route.Key == null)
        {
            // Pas de route trouvée, passer au middleware suivant
            await _next(context);
            return;
        }

        var targetUrl = route.Value;
        
        // Transformer le chemin pour les endpoints des services
        var transformedPath = path;
        
        // Pour les health checks : /api/[service]/health -> /health
        if (path.Contains("/health"))
        {
            transformedPath = "/health";
        }
        // Pour les endpoints API : /api/auth/users -> /api/users  
        else if (path.StartsWith("/api/auth/") && !path.StartsWith("/api/auth/health"))
        {
            transformedPath = path.Replace("/api/auth", "/api");
        }
        // Pour les autres services, enlever le préfixe service
        else if (path.StartsWith("/api/products/"))
        {
            transformedPath = path.Replace("/api/products", "/api/products");
        }
        else if (path.StartsWith("/api/stock/"))
        {
            transformedPath = path.Replace("/api/stock", "/api/stock");
        }
        else if (path.StartsWith("/api/orders/"))
        {
            transformedPath = path.Replace("/api/orders", "/api/orders");
        }
        else if (path.StartsWith("/api/payments/"))
        {
            transformedPath = path.Replace("/api/payments", "/api/payments");
        }
        
        var targetUri = $"{targetUrl}{transformedPath}{context.Request.QueryString}";

        _logger.LogInformation("Proxying request: {Method} {Path} -> {TargetUri} (transformed: {TransformedPath})", 
            context.Request.Method, path, targetUri, transformedPath);

        try
        {
            // Créer la requête HTTP vers le microservice
            var requestMessage = new HttpRequestMessage();
            requestMessage.Method = new HttpMethod(context.Request.Method);
            requestMessage.RequestUri = new Uri(targetUri);

            // Copier les headers
            foreach (var header in context.Request.Headers)
            {
                if (!IsSystemHeader(header.Key))
                {
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            // Copier le body pour POST/PUT
            if (context.Request.ContentLength > 0)
            {
                requestMessage.Content = new StreamContent(context.Request.Body);
                if (!string.IsNullOrEmpty(context.Request.ContentType))
                {
                    requestMessage.Content.Headers.TryAddWithoutValidation("Content-Type", context.Request.ContentType);
                }
            }

            // Envoyer la requête
            var responseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

            // Copier la réponse
            context.Response.StatusCode = (int)responseMessage.StatusCode;

            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            // Supprimer headers de transfert qui causent des problèmes
            context.Response.Headers.Remove("Transfer-Encoding");

            await responseMessage.Content.CopyToAsync(context.Response.Body);

            _logger.LogInformation("Proxy response: {StatusCode} for {Method} {Path}", 
                responseMessage.StatusCode, context.Request.Method, path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Proxy error for {Method} {Path} -> {TargetUri}", 
                context.Request.Method, path, targetUri);
            
            context.Response.StatusCode = 502; // Bad Gateway
            await context.Response.WriteAsync($"Gateway Error: Unable to reach {route.Value}");
        }
    }

    /// <summary>
    /// Vérifie si un header est un header système qui ne doit pas être transféré
    /// </summary>
    private bool IsSystemHeader(string headerName)
    {
        var systemHeaders = new[] 
        { 
            "Host", "Connection", "Transfer-Encoding", "Upgrade", 
            "Proxy-Connection", "Proxy-Authorization"
        };
        
        return systemHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }
}