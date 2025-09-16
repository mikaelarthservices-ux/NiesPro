using System;

namespace NiesPro.ServiceManager.Models
{
    public class MicroserviceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Port { get; set; }
        public string ProjectPath { get; set; } = string.Empty;
        public string BaseUrl => $"https://localhost:{Port}";
        public ServiceStatus Status { get; set; } = ServiceStatus.Stopped;
        public int? ProcessId { get; set; }
        public DateTime? LastStarted { get; set; }
        public string Icon { get; set; } = "🔧";
        public string Description { get; set; } = string.Empty;
        
        // Health check
        public bool IsHealthy { get; set; }
        public string HealthCheckUrl => $"{BaseUrl}/health";
        public string SwaggerUrl => $"{BaseUrl}/swagger";
        
        // Logs
        public string LogOutput { get; set; } = string.Empty;
    }

    public enum ServiceStatus
    {
        Stopped,
        Starting,
        Running,
        Stopping,
        Error
    }
}