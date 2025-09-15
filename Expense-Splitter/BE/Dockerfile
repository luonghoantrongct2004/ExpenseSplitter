# Expense-Splitter/BE/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution file
COPY ["BE.sln", "."]

# Copy all project files
COPY ["src/BE.API/BE.API.csproj", "src/BE.API/"]
COPY ["src/BE.Application/BE.Application.csproj", "src/BE.Application/"]
COPY ["src/BE.Domain/BE.Domain.csproj", "src/BE.Domain/"]
COPY ["src/BE.Infrastructure/BE.Infrastructure.csproj", "src/BE.Infrastructure/"]

# Restore
RUN dotnet restore "src/BE.API/BE.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/BE.API"
RUN dotnet build "BE.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BE.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "BE.API.dll"]
