# Dockerfile para BuildTruckBack (.NET)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["BuildTruckBack/BuildTruckBack.csproj", "BuildTruckBack/"]
RUN dotnet restore "BuildTruckBack/BuildTruckBack.csproj"
COPY . .
WORKDIR "/src/BuildTruckBack"
RUN dotnet build "BuildTruckBack.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BuildTruckBack.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BuildTruckBack.dll"]