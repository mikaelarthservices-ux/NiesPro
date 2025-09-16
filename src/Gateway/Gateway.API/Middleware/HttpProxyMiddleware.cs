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
        { "/api/auth", "https://localhost:5001" },
        { "/api/orders", "https://localhost:5002" },
        { "/api/products", "https://localhost:5003" },
        { "/api/categories", "https://localhost:5003" },
        { "/api/brands", "https://localhost:5003" }
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
        var targetUri = $"{targetUrl}{path}{context.Request.QueryString}";

        _logger.LogInformation("Proxying request: {Method} {Path} -> {TargetUri}", 
            context.Request.Method, path, targetUri);

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