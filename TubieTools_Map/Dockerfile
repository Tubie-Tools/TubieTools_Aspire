FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TubieTools_Map/TubieTools_Map.csproj", "TubieTools_Map/"]
COPY ["TubieTools_Aspire.ServiceDefaults/TubieTools_Aspire.ServiceDefaults.csproj", "TubieTools_Aspire.ServiceDefaults/"]

RUN dotnet restore "TubieTools_Map/TubieTools_Map.csproj"

COPY . .
WORKDIR "/src/TubieTools_Map"
RUN dotnet build "TubieTools_Map.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TubieTools_Map.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

ENV ASPNETCORE_URLS=http://+:8080

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TubieTools_Map.dll"]