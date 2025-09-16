using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NiesPro.ServiceAdmin.Models
{
    public enum ServiceStatus
    {
        Stopped,
        Starting,
        Running,
        Stopping,
        Error,
        Unknown
    }

    public enum ServiceHealth
    {
        Healthy,
        Degraded,
        Unhealthy,
        Unknown
    }

    public class MicroserviceInfo : INotifyPropertyChanged
    {
        private ServiceStatus _status = ServiceStatus.Stopped;
        private ServiceHealth _health = ServiceHealth.Unknown;
        private DateTime _lastHealthCheck = DateTime.MinValue;
        private string _lastError = string.Empty;
        private int _processId;
        private DateTime _startTime;
        private TimeSpan _uptime;

        public string Name { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int Port { get; }
        public string ProjectPath { get; }
        public string LogPath { get; }
        public string HealthEndpoint => $"http://localhost:{Port}/health";
        public string SwaggerEndpoint => $"http://localhost:{Port}/swagger";

        public ServiceStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusDisplay));
                    OnPropertyChanged(nameof(StatusColor));
                }
            }
        }

        public ServiceHealth Health
        {
            get => _health;
            set
            {
                if (_health != value)
                {
                    _health = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HealthDisplay));
                    OnPropertyChanged(nameof(HealthColor));
                }
            }
        }

        public DateTime LastHealthCheck
        {
            get => _lastHealthCheck;
            set
            {
                _lastHealthCheck = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LastHealthCheckDisplay));
            }
        }

        public string LastError
        {
            get => _lastError;
            set
            {
                _lastError = value;
                OnPropertyChanged();
            }
        }

        public int ProcessId
        {
            get => _processId;
            set
            {
                _processId = value;
                OnPropertyChanged();
            }
        }

        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                OnPropertyChanged();
                UpdateUptime();
            }
        }

        public TimeSpan Uptime
        {
            get => _uptime;
            set
            {
                _uptime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UptimeDisplay));
            }
        }

        // Display Properties
        public string StatusDisplay => Status.ToString();
        public string HealthDisplay => Health.ToString();
        public string StatusColor => Status switch
        {
            ServiceStatus.Running => "#4CAF50",
            ServiceStatus.Starting => "#FF9800",
            ServiceStatus.Stopping => "#FF9800",
            ServiceStatus.Error => "#F44336",
            ServiceStatus.Stopped => "#757575",
            _ => "#9E9E9E"
        };

        public string HealthColor => Health switch
        {
            ServiceHealth.Healthy => "#4CAF50",
            ServiceHealth.Degraded => "#FF9800",
            ServiceHealth.Unhealthy => "#F44336",
            _ => "#9E9E9E"
        };

        public string LastHealthCheckDisplay => LastHealthCheck == DateTime.MinValue 
            ? "Never" 
            : $"{LastHealthCheck:HH:mm:ss}";

        public string UptimeDisplay => Uptime.TotalSeconds < 1 
            ? "Not running" 
            : $"{Uptime.Days}d {Uptime.Hours:D2}h {Uptime.Minutes:D2}m {Uptime.Seconds:D2}s";

        public MicroserviceInfo(string name, string displayName, string description, int port, string projectPath)
        {
            Name = name;
            DisplayName = displayName;
            Description = description;
            Port = port;
            ProjectPath = projectPath;
            LogPath = $"logs/{name}.log";
        }

        public void UpdateUptime()
        {
            if (Status == ServiceStatus.Running && StartTime != DateTime.MinValue)
            {
                Uptime = DateTime.Now - StartTime;
            }
            else
            {
                Uptime = TimeSpan.Zero;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}