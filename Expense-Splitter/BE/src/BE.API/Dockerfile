FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file if exists
COPY *.sln ./

# Copy all csproj files and restore
COPY BE.API/*.csproj ./BE.API/
COPY BE.Application/*.csproj ./BE.Application/
COPY BE.Domain/*.csproj ./BE.Domain/
COPY BE.Infrastructure/*.csproj ./BE.Infrastructure/
COPY BE.Common/*.csproj ./BE.Common/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build
WORKDIR /src/BE.API
RUN dotnet build -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "BE.API.dll"]
