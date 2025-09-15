# Utilisation d'une image de base .NET 8 pour la runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Utilisation d'une image .NET 8 SDK pour le build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copie des fichiers de projet
COPY ["src/Services/Auth/Auth.API/Auth.API.csproj", "src/Services/Auth/Auth.API/"]

# Restauration des dépendances
RUN dotnet restore "src/Services/Auth/Auth.API/Auth.API.csproj"

# Copie de tout le code source
COPY . .
WORKDIR "/src/src/Services/Auth/Auth.API"

# Build de l'application
RUN dotnet build "Auth.API.csproj" -c Release -o /app/build

# Publication de l'application
FROM build AS publish
RUN dotnet publish "Auth.API.csproj" -c Release -o /app/publish

# Image finale
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.API.dll"]