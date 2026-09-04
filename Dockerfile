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

# Npgsql optionally loads libgssapi (for Kerberos/GSSAPI auth) at startup —
# it's not present in the slim base image and isn't actually needed for
# password auth, but Npgsql logs a scary-looking error for it regardless.
# Installing it keeps the logs clean instead of chasing a red herring.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Render (and most PaaS hosts) inject the port to listen on via $PORT rather
# than a fixed value, so Kestrel's binding is set at container start, not
# baked in at build time.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet JobTracker.dll"]
