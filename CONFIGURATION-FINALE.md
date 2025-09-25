# ⚙️ CONFIGURATION FINALE - NIESPRO ERP

*Configuration complète de production - 25 Septembre 2025*

---

## 🎯 **CONFIGURATION DES PORTS**

### **📊 Matrice Ports Finale**

| Service | HTTP | HTTPS | Base de Données | Statut | Health Check |
|---------|------|-------|-----------------|--------|--------------|
| **Gateway.API** | 5000 | 5010 | - | ✅ Production | /health |
| **Auth.API** | 5001 | 5011 | niespro_auth | ✅ Production | /health |
| **Order.API** | 5002 | 5012 | NiesPro_Order | ✅ Production | /health |
| **Catalog.API** | 5003 | 5013 | niespro_catalog | ✅ Production | /health |
| **Payment.API** | 5004 | 5014 | NiesPro_Payment | ✅ Production | /health |
| **Stock.API** | 5005 | 5015 | NiesPro_Stock | ✅ Production | /health |
| **Customer.API** | 5006 | 5016 | NiesPro_Customer | 🚧 Consolidation | /health |
| **Restaurant.API** | 5007 | 5017 | NiesPro_Restaurant | ✅ Production | /health |
| **Logs.API** | 5008 | 5018 | NiesPro_Logs | ✅ Production | /health |

### **🔧 Pattern de Ports**
```
HTTP:  50XX (5000, 5001, 5002, ...)
HTTPS: 50XX + 10 (5010, 5011, 5012, ...)
```

---

## 🛠️ **CONFIGURATION ENVIRONNEMENTS**

### **🏠 Développement Local**

#### **appsettings.Development.json (Template)**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "System.Net.Http.HttpClient": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database={{DATABASE_NAME}};Uid=root;Pwd=;",
    "ReadOnlyConnection": "Server=localhost;Port=3306;Database={{DATABASE_NAME}};Uid=root;Pwd=;"
  },
  "JWT": {
    "Key": "NiesProSuperSecretKeyForJWTAuthentication2024!",
    "Issuer": "NiesPro.AuthService", 
    "Audience": "NiesPro.Services",
    "ExpireMinutes": 60
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:{{HTTP_PORT}}"
      },
      "Https": {
        "Url": "https://localhost:{{HTTPS_PORT}}"
      }
    }
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "Http",
        "Args": {
          "requestUri": "https://localhost:5018/api/logs",
          "queueLimitBytes": null,
          "textFormatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
  },
  "LogsService": {
    "BaseUrl": "https://localhost:5018",
    "ApiKey": "dev-api-key-2024",
    "ServiceName": "{{SERVICE_NAME}}",
    "RetryAttempts": 3,
    "TimeoutSeconds": 30,
    "EnableHealthChecks": true
  },
  "HealthChecks": {
    "UI": {
      "HealthChecksUri": "/health",
      "HealthChecksUIUri": "/health-ui"
    }
  }
}
```

### **🚀 Production**

#### **appsettings.Production.json (Template)**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error",
      "System": "Error"
    }
  },
  "AllowedHosts": "*.niespro.com;localhost",
  "ConnectionStrings": {
    "DefaultConnection": "Server={{DB_SERVER}};Port=3306;Database={{DATABASE_NAME}};Uid={{DB_USER}};Pwd={{DB_PASSWORD}};SslMode=Required;",
    "ReadOnlyConnection": "Server={{DB_READONLY_SERVER}};Port=3306;Database={{DATABASE_NAME}};Uid={{DB_READONLY_USER}};Pwd={{DB_READONLY_PASSWORD}};SslMode=Required;"
  },
  "JWT": {
    "Key": "{{JWT_SECRET_KEY}}",
    "Issuer": "NiesPro.Production",
    "Audience": "NiesPro.ProductionServices",
    "ExpireMinutes": 30
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:{{HTTP_PORT}}"
      },
      "Https": {
        "Url": "https://0.0.0.0:{{HTTPS_PORT}}",
        "Certificate": {
          "Path": "/certs/{{SERVICE_NAME}}.pfx",
          "Password": "{{CERT_PASSWORD}}"
        }
      }
    },
    "Limits": {
      "MaxConcurrentConnections": 1000,
      "MaxRequestBodySize": 10485760,
      "RequestHeadersTimeout": "00:00:30"
    }
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Http",
        "Args": {
          "requestUri": "https://logs.niespro.com/api/logs",
          "queueLimitBytes": 1000000,
          "textFormatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "/logs/{{SERVICE_NAME}}/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId", "WithEnvironmentName" ]
  },
  "LogsService": {
    "BaseUrl": "https://logs.niespro.com",
    "ApiKey": "{{LOGS_API_KEY}}",
    "ServiceName": "{{SERVICE_NAME}}",
    "RetryAttempts": 5,
    "TimeoutSeconds": 60,
    "EnableHealthChecks": true
  },
  "RateLimiting": {
    "EnableLimiting": true,
    "Rules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 1000
      },
      {
        "Endpoint": "/api/auth/login",
        "Period": "1m", 
        "Limit": 10
      }
    ]
  }
}
```

---

## 🔐 **CONFIGURATION SÉCURITÉ**

### **🛡️ CORS Policy**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://localhost:3000",
      "https://admin.niespro.com",
      "https://pos.niespro.com"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    "AllowedHeaders": ["Content-Type", "Authorization", "X-Correlation-ID"],
    "AllowCredentials": true,
    "MaxAge": 3600
  }
}
```

### **🔒 JWT Configuration Avancée**
```json
{
  "JWT": {
    "Key": "{{JWT_SECRET_KEY_256_BITS}}",
    "Issuer": "NiesPro.AuthService",
    "Audience": "NiesPro.Services",
    "ExpireMinutes": 60,
    "RefreshTokenExpireDays": 30,
    "RequireHttpsMetadata": true,
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true,
    "ValidateIssuerSigningKey": true,
    "ClockSkew": "00:01:00"
  }
}
```

---

## 🚀 **CONFIGURATION GATEWAY**

### **⚙️ Gateway appsettings.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5010"
      }
    }
  },
  "JWT": {
    "Key": "NiesProSuperSecretKeyForJWTAuthentication2024!",
    "Issuer": "NiesPro.AuthService",
    "Audience": "NiesPro.Services",
    "ExpireMinutes": 60
  },
  "Microservices": {
    "AuthAPI": {
      "BaseUrl": "https://localhost:5011",
      "HealthEndpoint": "/health",
      "Timeout": 30
    },
    "OrderAPI": {
      "BaseUrl": "https://localhost:5012",
      "HealthEndpoint": "/health",
      "Timeout": 30
    },
    "CatalogAPI": {
      "BaseUrl": "https://localhost:5013",
      "HealthEndpoint": "/health",
      "Timeout": 30
    },
    "PaymentAPI": {
      "BaseUrl": "https://localhost:5014",
      "HealthEndpoint": "/health",
      "Timeout": 30
    },
    "StockAPI": {
      "BaseUrl": "https://localhost:5015",
      "HealthEndpoint": "/health",
      "Timeout": 30
    },
    "CustomerAPI": {
      "BaseUrl": "https://localhost:5016",
      "HealthEndpoint": "/health",
      "Timeout": 30
    },
    "RestaurantAPI": {
      "BaseUrl": "https://localhost:5017",
      "HealthEndpoint": "/health",
      "Timeout": 30
    },
    "LogsAPI": {
      "BaseUrl": "https://localhost:5018",
      "HealthEndpoint": "/health",
      "Timeout": 10
    }
  },
  "RateLimiting": {
    "GeneralRules": {
      "PermitLimit": 1000,
      "Window": "00:01:00"
    },
    "AuthRules": {
      "PermitLimit": 10,
      "Window": "00:01:00"
    }
  },
  "CircuitBreaker": {
    "HandledEventsAllowedBeforeBreaking": 3,
    "DurationOfBreak": "00:00:30",
    "SamplingDuration": "00:02:00",
    "MinimumThroughput": 5
  }
}
```

---

## 💾 **CONFIGURATION BASE DE DONNÉES**

### **🗄️ Chaînes de Connexion par Environnement**

#### **Développement Local**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database={{DB_NAME}};Uid=root;Pwd=;CharSet=utf8mb4;SslMode=none;AllowPublicKeyRetrieval=true;",
    "ReadOnlyConnection": "Server=localhost;Port=3306;Database={{DB_NAME}};Uid=root;Pwd=;CharSet=utf8mb4;SslMode=none;AllowPublicKeyRetrieval=true;"
  }
}
```

#### **Production**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server={{PRIMARY_DB}};Port=3306;Database={{DB_NAME}};Uid={{DB_USER}};Pwd={{DB_PASSWORD}};CharSet=utf8mb4;SslMode=Required;SslCert=/certs/client-cert.pem;SslKey=/certs/client-key.pem;SslCa=/certs/ca-cert.pem;",
    "ReadOnlyConnection": "Server={{READONLY_DB}};Port=3306;Database={{DB_NAME}};Uid={{READONLY_USER}};Pwd={{READONLY_PASSWORD}};CharSet=utf8mb4;SslMode=Required;"
  }
}
```

### **📊 Configuration EF Core**
```json
{
  "EntityFramework": {
    "CommandTimeout": 30,
    "EnableSensitiveDataLogging": false,
    "EnableServiceProviderCaching": true,
    "EnableDetailedErrors": false,
    "LazyLoadingEnabled": false,
    "QueryTrackingBehavior": "NoTracking",
    "AutoDetectChangesEnabled": true,
    "ValidateOnSaveEnabled": true
  }
}
```

---

## 🐳 **CONFIGURATION DOCKER**

### **📦 docker-compose.yml (Développement)**
```yaml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: niespro_dev
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
      - ./scripts/init.sql:/docker-entrypoint-initdb.d/init.sql

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.10.0
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
    ports:
      - "9200:9200"
      - "9300:9300"
    volumes:
      - elasticsearch_data:/usr/share/elasticsearch/data

  gateway:
    build:
      context: ./src/Services/Gateway
      dockerfile: Dockerfile
    ports:
      - "5000:5000"
      - "5010:5010"
    depends_on:
      - mysql
      - elasticsearch
    environment:
      - ASPNETCORE_ENVIRONMENT=Development

  auth:
    build:
      context: ./src/Services/Auth
      dockerfile: Dockerfile
    ports:
      - "5001:5001"
      - "5011:5011"
    depends_on:
      - mysql
    environment:
      - ASPNETCORE_ENVIRONMENT=Development

volumes:
  mysql_data:
  elasticsearch_data:
```

### **🚀 docker-compose.production.yml**
```yaml
version: '3.8'

services:
  gateway:
    image: niespro/gateway:${TAG:-latest}
    ports:
      - "80:5000"
      - "443:5010"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - JWT__Key=${JWT_SECRET}
    volumes:
      - ./certs:/certs:ro
      - ./logs:/logs
    restart: unless-stopped

  auth:
    image: niespro/auth:${TAG:-latest}
    ports:
      - "5011:5011"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${AUTH_DB_CONNECTION}
    volumes:
      - ./logs:/logs
    restart: unless-stopped

  # Autres services...

networks:
  niespro-network:
    driver: bridge

volumes:
  logs:
  certs:
```

---

## 🔧 **SCRIPTS DE DÉMARRAGE**

### **🚀 start-all-services.ps1**
```powershell
#!/usr/bin/env pwsh

Write-Host "🚀 Démarrage de tous les services NiesPro ERP..." -ForegroundColor Green

# Variables
$services = @(
    @{ Name="Gateway"; Path="src/Services/Gateway/Gateway.API"; Port=5010 },
    @{ Name="Auth"; Path="src/Services/Auth/Auth.API"; Port=5011 },
    @{ Name="Logs"; Path="src/Services/Logs/Logs.API"; Port=5018 },
    @{ Name="Catalog"; Path="src/Services/Catalog/Catalog.API"; Port=5013 },
    @{ Name="Stock"; Path="src/Services/Stock/Stock.API"; Port=5015 },
    @{ Name="Customer"; Path="src/Services/Customer/Customer.API"; Port=5016 },
    @{ Name="Order"; Path="src/Services/Order/Order.API"; Port=5012 },
    @{ Name="Payment"; Path="src/Services/Payment/Payment.API"; Port=5014 },
    @{ Name="Restaurant"; Path="src/Services/Restaurant/Restaurant.API"; Port=5017 }
)

# Démarrage des services en arrière-plan
foreach ($service in $services) {
    Write-Host "Démarrage du service $($service.Name)..." -ForegroundColor Yellow
    Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", $service.Path -WindowStyle Hidden
    Start-Sleep -Seconds 2
}

# Vérification des services
Write-Host "Attente du démarrage des services..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

Write-Host "Vérification des services..." -ForegroundColor Yellow
foreach ($service in $services) {
    try {
        $response = Invoke-WebRequest -Uri "https://localhost:$($service.Port)/health" -SkipCertificateCheck
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ $($service.Name) - OK" -ForegroundColor Green
        } else {
            Write-Host "❌ $($service.Name) - ERREUR" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "❌ $($service.Name) - INACCESSIBLE" -ForegroundColor Red
    }
}

Write-Host "🎯 Gateway disponible sur: https://localhost:5010/swagger" -ForegroundColor Cyan
Write-Host "📊 Health checks: https://localhost:5010/health" -ForegroundColor Cyan
```

### **🛑 stop-all-services.ps1**
```powershell
#!/usr/bin/env pwsh

Write-Host "🛑 Arrêt de tous les services NiesPro ERP..." -ForegroundColor Red

# Arrêt des processus dotnet
Get-Process | Where-Object { $_.ProcessName -eq "dotnet" -and $_.MainWindowTitle -like "*NiesPro*" } | Stop-Process -Force

Write-Host "✅ Tous les services ont été arrêtés." -ForegroundColor Green
```

---

## 📋 **CHECKLIST DE DÉPLOIEMENT**

### **✅ Pré-Production**
- [ ] Tous les services démarrent sans erreur
- [ ] Health checks passent (200 OK)
- [ ] JWT authentication fonctionne
- [ ] Bases de données accessibles
- [ ] Logs centralisés actifs
- [ ] Tests d'intégration passent

### **🚀 Production**  
- [ ] Certificats SSL configurés
- [ ] Variables d'environnement sécurisées
- [ ] Monitoring actif (logs, métriques)
- [ ] Sauvegardes automatiques configurées
- [ ] Rate limiting activé
- [ ] Circuit breakers configurés
- [ ] Load balancer configuré

---

**🎯 Cette configuration finale garantit un déploiement sécurisé, performant et maintenable de la plateforme NiesPro ERP.**