# Multi-stage Dockerfile for TubieTools Forecasting API

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app

RUN apk add --no-cache curl

RUN addgroup -g 1001 -S appgroup && \
	adduser -u 1001 -S appuser -G appgroup

EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["TubieTools_Forecasting_API/TubieTools_Forecasting_API.csproj", "TubieTools_Forecasting_API/"]
COPY ["ServiceLayer/ServiceLayer.csproj", "ServiceLayer/"]
COPY ["DataAccessLayer/DataAccessLayer.csproj", "DataAccessLayer/"]
COPY ["ModelLayer/ModelLayer.csproj", "ModelLayer/"]
COPY ["DTOLayer/DTOLayer.csproj", "DTOLayer/"]
COPY ["TubieTools_Aspire.ServiceDefaults/TubieTools_Aspire.ServiceDefaults.csproj", "TubieTools_Aspire.ServiceDefaults/"]

RUN dotnet restore "TubieTools_Forecasting_API/TubieTools_Forecasting_API.csproj"

COPY . .

WORKDIR "/src/TubieTools_Forecasting_API"
RUN dotnet build "TubieTools_Forecasting_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TubieTools_Forecasting_API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

USER appuser

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "TubieTools_Forecasting_API.dll"]
