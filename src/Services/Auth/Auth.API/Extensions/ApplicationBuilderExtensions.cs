namespace Auth.API.Extensions
{
    /// <summary>
    /// Application middleware configuration extensions
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Configure the HTTP request pipeline for Auth API
        /// </summary>
        public static WebApplication ConfigureAuthApiPipeline(this WebApplication app)
        {
            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NiesPro Auth API v1");
                    c.RoutePrefix = "swagger";
                    c.DisplayRequestDuration();
                    c.EnableDeepLinking();
                    c.EnableFilter();
                    c.ShowExtensions();
                });
            }

            // Security headers
            app.UseSecurityHeaders();

            // HTTPS redirection
            app.UseHttpsRedirection();

            // CORS
            app.UseCors("AllowFrontend");

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Exception handling
            app.UseGlobalExceptionHandler();

            // Health checks
            app.MapHealthChecks("/health");
            app.MapHealthChecks("/health/ready");
            app.MapHealthChecks("/health/live");

            // API routes
            app.MapControllers();

            return app;
        }

        /// <summary>
        /// Add security headers middleware
        /// </summary>
        private static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                // Add security headers
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Append("Content-Security-Policy", 
                    "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline';");

                await next();
            });
        }

        /// <summary>
        /// Add global exception handling middleware
        /// </summary>
        private static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    var errorResponse = new
                    {
                        error = "An internal server error occurred",
                        timestamp = DateTime.UtcNow,
                        path = context.Request.Path.Value
                    };

                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
                });
            });
        }

        /// <summary>
        /// Configure le pipeline middleware de l'API (alias pour ConfigureAuthApiPipeline)
        /// </summary>
        public static IApplicationBuilder UseApiMiddleware(this IApplicationBuilder app)
        {
            var webApp = (WebApplication)app;
            webApp.ConfigureAuthApiPipeline();
            return app;
        }
    }
}