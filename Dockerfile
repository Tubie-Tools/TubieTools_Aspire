FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["TubieTools_Aspire.AppHost/TubieTools_Aspire.AppHost.csproj", "TubieTools_Aspire.AppHost/"]
COPY ["TubieTools_Aspire.ServiceDefaults/TubieTools_Aspire.ServiceDefaults.csproj", "TubieTools_Aspire.ServiceDefaults/"]
COPY ["TubieTools_Aspire.Web/TubieTools_Aspire.Web.csproj", "TubieTools_Aspire.Web/"]
COPY ["TubieTools_PublicAPI/TubieTools_PublicAPI.csproj", "TubieTools_PublicAPI/"]
COPY ["TubieTools_TensorFlow/TubieTools_TensorFlow.csproj", "TubieTools_TensorFlow/"]
COPY ["TubieTools_Machine_Learning/TubieTools_Machine_Learning.csproj", "TubieTools_Machine_Learning/"]
COPY ["TubieTools_Forecasting_API/TubieTools_Forecasting_API.csproj", "TubieTools_Forecasting_API/"]
COPY ["TubieTools_SentimentModel_ProductApi/TubieTools_SentimentModel_ProductApi.csproj", "TubieTools_SentimentModel_ProductApi/"]
COPY ["TubieTools_SentimentModel_WebApi/TubieTools_SentimentModel_WebApi.csproj", "TubieTools_SentimentModel_WebApi/"]
COPY ["TubieTools_Converter/TubieTools_Converter.csproj", "TubieTools_Converter/"]
COPY ["TubieTools_PublicAPI/TubieTools_PublicAPI.csproj", "TubieTools_PublicAPI/"]
COPY ["DTOLayer/DTOLayer.csproj", "DTOLayer/"]
COPY ["ModelLayer/ModelLayer.csproj", "ModelLayer/"]
COPY ["DataAccessLayer/DataAccessLayer.csproj", "DataAccessLayer/"]
COPY ["ServiceLayer/ServiceLayer.csproj", "ServiceLayer/"]

WORKDIR /src
RUN dotnet restore "TubieTools_Aspire.AppHost/TubieTools_Aspire.AppHost.csproj"

# Copy remaining source code
COPY . .

# Build
WORKDIR /src
RUN dotnet build "TubieTools_Aspire.AppHost/TubieTools_Aspire.AppHost.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "TubieTools_Aspire.AppHost/TubieTools_Aspire.AppHost.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 5000 5001 8080 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000;https://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "TubieTools_Aspire.AppHost.dll"]
