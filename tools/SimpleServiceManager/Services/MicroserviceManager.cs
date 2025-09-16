using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NiesPro.ServiceAdmin.Models;
using Newtonsoft.Json;

namespace NiesPro.ServiceAdmin.Services
{
    public interface IMicroserviceManager : IDisposable
    {
        Task<bool> StartServiceAsync(MicroserviceInfo service, CancellationToken cancellationToken = default);
        Task<bool> StopServiceAsync(MicroserviceInfo service, CancellationToken cancellationToken = default);
        Task<bool> RestartServiceAsync(MicroserviceInfo service, CancellationToken cancellationToken = default);
        Task UpdateServiceStatusAsync(MicroserviceInfo service, CancellationToken cancellationToken = default);
        Task<ServiceHealth> CheckServiceHealthAsync(MicroserviceInfo service, CancellationToken cancellationToken = default);
    }

    public class MicroserviceManager : IMicroserviceManager, IDisposable
    {
        private readonly ILogger<MicroserviceManager> _logger;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, Process> _processes;
        private readonly Timer? _healthCheckTimer;
        private readonly SemaphoreSlim _semaphore;

        public MicroserviceManager(ILogger<MicroserviceManager> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _processes = new Dictionary<string, Process>();
            _semaphore = new SemaphoreSlim(1, 1);
            
            // Configure HttpClient
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<bool> StartServiceAsync(MicroserviceInfo service, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Starting service {ServiceName} on port {Port}", service.Name, service.Port);

                // Check if already running
                if (_processes.ContainsKey(service.Name))
                {
                    var existingProcess = _processes[service.Name];
                    if (existingProcess != null && !existingProcess.HasExited)
                    {
                        _logger.LogWarning("Service {ServiceName} is already running with PID {ProcessId}", 
                            service.Name, existingProcess.Id);
                        return true;
                    }
                }

                // Validate project path
                if (!Directory.Exists(service.ProjectPath))
                {
                    _logger.LogError("Project directory not found: {ProjectPath}", service.ProjectPath);
                    service.Status = ServiceStatus.Error;
                    service.LastError = $"Project directory not found: {service.ProjectPath}";
                    return false;
                }

                service.Status = ServiceStatus.Starting;
                service.LastError = string.Empty;

                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run",
                    WorkingDirectory = service.ProjectPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Environment = 
                    {
                        ["ASPNETCORE_ENVIRONMENT"] = "Development",
                        ["ASPNETCORE_URLS"] = $"http://localhost:{service.Port}"
                    }
                };

                var process = new Process { StartInfo = startInfo };
                
                // Setup logging
                var logFile = Path.Combine(Directory.GetCurrentDirectory(), "logs", $"{service.Name}.log");
                Directory.CreateDirectory(Path.GetDirectoryName(logFile)!);

                process.OutputDataReceived += (sender, e) => LogProcessOutput(service.Name, "OUT", e.Data, logFile);
                process.ErrorDataReceived += (sender, e) => LogProcessOutput(service.Name, "ERR", e.Data, logFile);
                
                process.Exited += (sender, e) =>
                {
                    _logger.LogInformation("Service {ServiceName} exited with code {ExitCode}", 
                        service.Name, process.ExitCode);
                    service.Status = ServiceStatus.Stopped;
                    service.ProcessId = 0;
                    _processes.Remove(service.Name);
                };

                process.EnableRaisingEvents = true;

                // Start the process
                if (!process.Start())
                {
                    _logger.LogError("Failed to start process for service {ServiceName}", service.Name);
                    service.Status = ServiceStatus.Error;
                    service.LastError = "Failed to start process";
                    return false;
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                _processes[service.Name] = process;
                service.ProcessId = process.Id;
                service.StartTime = DateTime.Now;

                _logger.LogInformation("Service {ServiceName} started with PID {ProcessId}", 
                    service.Name, process.Id);

                // Wait for service to initialize
                await Task.Delay(3000, cancellationToken);

                // Check if still running
                if (!process.HasExited)
                {
                    service.Status = ServiceStatus.Running;
                    _logger.LogInformation("Service {ServiceName} is running successfully", service.Name);
                    return true;
                }
                else
                {
                    service.Status = ServiceStatus.Error;
                    service.LastError = $"Service exited during startup with code {process.ExitCode}";
                    _logger.LogError("Service {ServiceName} exited during startup with code {ExitCode}", 
                        service.Name, process.ExitCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting service {ServiceName}", service.Name);
                service.Status = ServiceStatus.Error;
                service.LastError = ex.Message;
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> StopServiceAsync(MicroserviceInfo service, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Stopping service {ServiceName}", service.Name);

                if (!_processes.ContainsKey(service.Name))
                {
                    _logger.LogWarning("No process found for service {ServiceName}", service.Name);
                    service.Status = ServiceStatus.Stopped;
                    return true;
                }

                var process = _processes[service.Name];
                if (process == null || process.HasExited)
                {
                    _logger.LogInformation("Service {ServiceName} is already stopped", service.Name);
                    service.Status = ServiceStatus.Stopped;
                    service.ProcessId = 0;
                    _processes.Remove(service.Name);
                    return true;
                }

                service.Status = ServiceStatus.Stopping;

                // Graceful shutdown first
                try
                {
                    process.CloseMainWindow();
                    if (await WaitForExitAsync(process, TimeSpan.FromSeconds(10)))
                    {
                        _logger.LogInformation("Service {ServiceName} stopped gracefully", service.Name);
                    }
                    else
                    {
                        _logger.LogWarning("Service {ServiceName} did not stop gracefully, forcing termination", service.Name);
                        process.Kill(true);
                        await WaitForExitAsync(process, TimeSpan.FromSeconds(5));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during graceful shutdown of {ServiceName}, forcing termination", service.Name);
                    process.Kill(true);
                    await WaitForExitAsync(process, TimeSpan.FromSeconds(5));
                }

                service.Status = ServiceStatus.Stopped;
                service.ProcessId = 0;
                service.Uptime = TimeSpan.Zero;
                _processes.Remove(service.Name);

                _logger.LogInformation("Service {ServiceName} stopped successfully", service.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping service {ServiceName}", service.Name);
                service.LastError = ex.Message;
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> RestartServiceAsync(MicroserviceInfo service, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Restarting service {ServiceName}", service.Name);
            
            var stopResult = await StopServiceAsync(service, cancellationToken);
            if (!stopResult)
            {
                return false;
            }

            await Task.Delay(2000, cancellationToken); // Wait between stop and start
            
            return await StartServiceAsync(service, cancellationToken);
        }

        public async Task UpdateServiceStatusAsync(MicroserviceInfo service, CancellationToken cancellationToken = default)
        {
            try
            {
                // Update uptime if running
                if (service.Status == ServiceStatus.Running)
                {
                    service.UpdateUptime();
                }

                // Check process status
                if (_processes.ContainsKey(service.Name))
                {
                    var process = _processes[service.Name];
                    if (process != null && process.HasExited)
                    {
                        _logger.LogInformation("Detected that service {ServiceName} has exited", service.Name);
                        service.Status = ServiceStatus.Stopped;
                        service.ProcessId = 0;
                        _processes.Remove(service.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error updating status for service {ServiceName}", service.Name);
            }
        }

        public async Task<ServiceHealth> CheckServiceHealthAsync(MicroserviceInfo service, CancellationToken cancellationToken = default)
        {
            try
            {
                if (service.Status != ServiceStatus.Running)
                {
                    return ServiceHealth.Unhealthy;
                }

                var response = await _httpClient.GetAsync(service.HealthEndpoint, cancellationToken);
                service.LastHealthCheck = DateTime.Now;

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    
                    // Try to parse health response
                    try
                    {
                        var healthData = JsonConvert.DeserializeObject<dynamic>(content);
                        var status = healthData?.status?.ToString()?.ToLowerInvariant();
                        
                        service.Health = status switch
                        {
                            "healthy" => ServiceHealth.Healthy,
                            "degraded" => ServiceHealth.Degraded,
                            "unhealthy" => ServiceHealth.Unhealthy,
                            _ => ServiceHealth.Unknown
                        };
                    }
                    catch
                    {
                        service.Health = ServiceHealth.Healthy; // Assume healthy if we got 200 OK
                    }
                }
                else
                {
                    service.Health = ServiceHealth.Unhealthy;
                }

                return service.Health;
            }
            catch (TaskCanceledException)
            {
                service.Health = ServiceHealth.Unknown;
                return ServiceHealth.Unknown;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check failed for service {ServiceName}", service.Name);
                service.Health = ServiceHealth.Unknown;
                return ServiceHealth.Unknown;
            }
        }

        private void LogProcessOutput(string serviceName, string type, string? data, string logFile)
        {
            if (string.IsNullOrWhiteSpace(data)) return;

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] [{type}] {data}";
            
            try
            {
                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to write to log file {LogFile}", logFile);
            }
        }

        private static async Task<bool> WaitForExitAsync(Process process, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<bool>();
            
            void ProcessExited(object? sender, EventArgs e) => tcs.TrySetResult(true);
            
            process.Exited += ProcessExited;
            
            try
            {
                if (process.HasExited)
                    return true;

                using var timeoutCts = new CancellationTokenSource(timeout);
                using var registration = timeoutCts.Token.Register(() => tcs.TrySetResult(false));
                
                return await tcs.Task;
            }
            finally
            {
                process.Exited -= ProcessExited;
            }
        }

        public void Dispose()
        {
            _healthCheckTimer?.Dispose();
            
            foreach (var process in _processes.Values)
            {
                try
                {
                    if (process != null && !process.HasExited)
                    {
                        process.Kill(true);
                        process.WaitForExit(5000);
                    }
                    process?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing process");
                }
            }
            
            _processes.Clear();
            _semaphore?.Dispose();
            _httpClient?.Dispose();
        }
    }
}