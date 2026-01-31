# Étape de build .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

# On copie tout et on restaure les dépendances
COPY . .
RUN dotnet restore

# On publie l'application en mode Release
RUN dotnet publish src/Mo5.RagServer.Api/Mo5.RagServer.Api.csproj -c Release -o /app/publish

# Étape finale (Runtime) - ON NE GARDE QUE LE NECESSAIRE
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
WORKDIR /app

# Plus besoin de Python, torch ou sentence-transformers ici !
# On garde juste curl pour les healthchecks si besoin
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copie du binaire .NET depuis l'étape de build
COPY --from=build /app/publish .

# On garde le dossier knowledge si l'API doit lire les fichiers MD
RUN mkdir -p /app/knowledge

# Port d'écoute par défaut de l'API
EXPOSE 8080

ENTRYPOINT ["dotnet", "Mo5.RagServer.Api.dll"]