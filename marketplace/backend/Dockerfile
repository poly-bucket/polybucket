FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file
COPY PolyBucket.Marketplace.Api/*.csproj ./PolyBucket.Marketplace.Api/

# Restore dependencies
RUN dotnet restore PolyBucket.Marketplace.Api/PolyBucket.Marketplace.Api.csproj

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR /app/PolyBucket.Marketplace.Api
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
RUN chown -R app:app /app

# Install curl for healthchecks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:10280/health || exit 1

EXPOSE 10280
EXPOSE 443

ENTRYPOINT ["dotnet", "run", "--project", "PolyBucket.Marketplace.Api"]
