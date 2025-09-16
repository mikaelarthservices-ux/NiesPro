using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NiesPro.ServiceManager.Models;

namespace NiesPro.ServiceManager.Services
{
    public class ServiceController
    {
        private readonly string _basePath;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, Process> _runningProcesses;

        public ServiceController(string basePath)
        {
            _basePath = basePath;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
            _runningProcesses = new Dictionary<string, Process>();
        }

        public List<MicroserviceInfo> GetServices()
        {
            return new List<MicroserviceInfo>
            {
                new MicroserviceInfo
                {
                    Name = "Gateway",
                    DisplayName = "🌐 Gateway API",
                    Port = 5000,
                    ProjectPath = Path.Combine(_basePath, "src", "Gateway", "Gateway.API"),
                    Icon = "🌐",
                    Description = "Point d'entrée principal - Routage intelligent et authentification"
                },
                new MicroserviceInfo
                {
                    Name = "Auth",
                    DisplayName = "🔐 Auth API",
                    Port = 5001,
                    ProjectPath = Path.Combine(_basePath, "src", "Services", "Auth", "Auth.API"),
                    Icon = "🔐",
                    Description = "Service d'authentification JWT et gestion des utilisateurs"
                },
                new MicroserviceInfo
                {
                    Name = "Order",
                    DisplayName = "📋 Order API",
                    Port = 5002,
                    ProjectPath = Path.Combine(_basePath, "src", "Services", "Order", "Order.API"),
                    Icon = "📋",
                    Description = "Gestion des commandes avec Event Sourcing CQRS"
                },
                new MicroserviceInfo
                {
                    Name = "Catalog",
                    DisplayName = "📦 Catalog API",
                    Port = 5003,
                    ProjectPath = Path.Combine(_basePath, "src", "Services", "Catalog", "Catalog.API"),
                    Icon = "📦",
                    Description = "Catalogue produits avec architecture Clean Architecture"
                }
            };
        }

        public async Task<bool> StartServiceAsync(MicroserviceInfo service)
        {
            try
            {
                if (_runningProcesses.ContainsKey(service.Name))
                {
                    return false; // Already running
                }

                // Ensure project path exists
                if (!Directory.Exists(service.ProjectPath))
                {
                    service.LogOutput += $"[{DateTime.Now:HH:mm:ss}] ERROR: Project path not found: {service.ProjectPath}\n";
                    service.Status = ServiceStatus.Error;
                    return false;
                }

                service.LogOutput += $"[{DateTime.Now:HH:mm:ss}] Starting {service.Name} on {service.BaseUrl}...\n";
                service.LogOutput += $"[{DateTime.Now:HH:mm:ss}] Working directory: {service.ProjectPath}\n";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --urls \"{service.BaseUrl}\"",
                    WorkingDirectory = service.ProjectPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = startInfo };
                
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        service.LogOutput += $"[{DateTime.Now:HH:mm:ss}] OUT: {e.Data}\n";
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        service.LogOutput += $"[{DateTime.Now:HH:mm:ss}] ERR: {e.Data}\n";
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                _runningProcesses[service.Name] = process;
                service.ProcessId = process.Id;
                service.LastStarted = DateTime.Now;
                service.Status = ServiceStatus.Starting;

                service.LogOutput += $"[{DateTime.Now:HH:mm:ss}] Process started with PID: {process.Id}\n";

                // Wait a bit and check if it's really running
                await Task.Delay(5000);
                
                if (!process.HasExited)
                {
                    // Double-check with health endpoint
                    await Task.Delay(2000);
                    var isHealthy = await CheckHealthAsync(service);
                    
                    if (isHealthy)
                    {
                        service.Status = ServiceStatus.Running;
                        service.LogOutput += $"[{DateTime.Now:HH:mm:ss}] Service is healthy and running!\n";
                        return true;
                    }
                    else
                    {
                        service.LogOutput += $"[{DateTime.Now:HH:mm:ss}] WARN: Service started but health check failed\n";
                        service.Status = ServiceStatus.Error;
                        // Kill the process if health check fails
                        try { process.Kill(); } catch { }
                        _runningProcesses.Remove(service.Name);
                        return false;
                    }
                }
                else
                {
                    service.Status = ServiceStatus.Error;
                    service.LogOutput += $"[{DateTime.Now:HH:mm:ss}] ERROR: Process exited immediately with code {process.ExitCode}\n";
                    _runningProcesses.Remove(service.Name);
                    return false;
                }
            }
            catch (Exception ex)
            {
                service.LogOutput += $"[{DateTime.Now:HH:mm:ss}] EXCEPTION: {ex.Message}\n";
                service.LogOutput += $"[{DateTime.Now:HH:mm:ss}] Stack trace: {ex.StackTrace}\n";
                service.Status = ServiceStatus.Error;
                return false;
            }
        }

        public async Task<bool> StopServiceAsync(MicroserviceInfo service)
        {
            try
            {
                if (!_runningProcesses.ContainsKey(service.Name))
                {
                    return false;
                }

                var process = _runningProcesses[service.Name];
                service.Status = ServiceStatus.Stopping;

                if (!process.HasExited)
                {
                    process.Kill();
                    await Task.Delay(1000);
                }

                _runningProcesses.Remove(service.Name);
                service.ProcessId = null;
                service.Status = ServiceStatus.Stopped;
                service.IsHealthy = false;

                return true;
            }
            catch (Exception ex)
            {
                service.LogOutput += $"[{DateTime.Now:HH:mm:ss}] STOP ERROR: {ex.Message}\n";
                return false;
            }
        }

        public async Task<bool> CheckHealthAsync(MicroserviceInfo service)
        {
            try
            {
                var response = await _httpClient.GetAsync(service.HealthCheckUrl);
                service.IsHealthy = response.IsSuccessStatusCode;
                return service.IsHealthy;
            }
            catch
            {
                service.IsHealthy = false;
                return false;
            }
        }

        public async Task StopAllServicesAsync(List<MicroserviceInfo> services)
        {
            var tasks = services.Where(s => s.Status == ServiceStatus.Running)
                              .Select(s => StopServiceAsync(s));
            await Task.WhenAll(tasks);
        }

        public void Dispose()
        {
            foreach (var process in _runningProcesses.Values)
            {
                try
                {
                    if (!process.HasExited)
                        process.Kill();
                }
                catch { }
            }
            _httpClient?.Dispose();
        }
    }
}