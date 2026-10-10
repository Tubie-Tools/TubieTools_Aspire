# Multi-stage Dockerfile for TubieTools.PublicAPI
# Production-ready with optimized build and runtime

# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app

# Install curl for health checks
RUN apk add --no-cache curl

# Create non-root user for security
RUN addgroup -g 1001 -S appgroup && \
	adduser -u 1001 -S appuser -G appgroup

# Expose ports
EXPOSE 80
EXPOSE 443

# Stage 2: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy project files
COPY ["TubieTools_PublicAPI/TubieTools_PublicAPI.csproj", "TubieTools_PublicAPI/"]
COPY ["ServiceLayer/ServiceLayer.csproj", "ServiceLayer/"]
COPY ["DataAccessLayer/DataAccessLayer.csproj", "DataAccessLayer/"]
COPY ["ModelLayer/ModelLayer.csproj", "ModelLayer/"]
COPY ["DTOLayer/DTOLayer.csproj", "DTOLayer/"]
COPY ["TubieTools_Aspire.ServiceDefaults/TubieTools_Aspire.ServiceDefaults.csproj", "TubieTools_Aspire.ServiceDefaults/"]

# Restore dependencies
RUN dotnet restore "TubieTools_PublicAPI/TubieTools_PublicAPI.csproj"

# Copy all source files
COPY . .

# Build application
WORKDIR "/src/TubieTools_PublicAPI"
RUN dotnet build "TubieTools_PublicAPI.csproj" -c Release -o /app/build

# Stage 3: Publish stage
FROM build AS publish
RUN dotnet publish "TubieTools_PublicAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Set user (non-root for security)
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "TubieTools_PublicAPI.dll"]
