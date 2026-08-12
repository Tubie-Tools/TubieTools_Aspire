# Dockerfile for MapApp.API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MapApp.API.csproj", "./"]
RUN dotnet restore "MapApp.API.csproj"

COPY . .
RUN dotnet build "MapApp.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MapApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5000 5001
ENTRYPOINT ["dotnet", "MapApp.API.dll"]
