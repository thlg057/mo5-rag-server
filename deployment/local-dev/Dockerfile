# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install curl for health checks in build stage
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy solution and project files
COPY Mo5.RagServer.sln ./
COPY src/Mo5.RagServer.Api/Mo5.RagServer.Api.csproj src/Mo5.RagServer.Api/
COPY src/Mo5.RagServer.Core/Mo5.RagServer.Core.csproj src/Mo5.RagServer.Core/
COPY src/Mo5.RagServer.Infrastructure/Mo5.RagServer.Infrastructure.csproj src/Mo5.RagServer.Infrastructure/
COPY tests/Mo5.RagServer.Tests/Mo5.RagServer.Tests.csproj tests/Mo5.RagServer.Tests/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/ src/
COPY tests/ tests/

# Build and test
RUN dotnet build --no-restore -c Release
RUN dotnet test --no-build -c Release --verbosity normal

# Publish
RUN dotnet publish src/Mo5.RagServer.Api/Mo5.RagServer.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r mo5user && useradd -r -g mo5user mo5user

# Copy published app
COPY --from=build /app/publish .

# Create knowledge directory and set permissions
RUN mkdir -p /app/knowledge && chown -R mo5user:mo5user /app

# Switch to non-root user
USER mo5user

EXPOSE 8080

ENTRYPOINT ["dotnet", "Mo5.RagServer.Api.dll"]
