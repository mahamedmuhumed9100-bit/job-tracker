# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY JobTracker.csproj .
RUN dotnet restore JobTracker.csproj

COPY . .
RUN dotnet publish JobTracker.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render (and most PaaS hosts) inject the port to listen on via $PORT rather
# than a fixed value, so Kestrel's binding is set at container start, not
# baked in at build time.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet JobTracker.dll"]
