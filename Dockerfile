# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CafeteriaApi.csproj", "./"]
RUN dotnet restore "CafeteriaApi.csproj"
COPY . .
RUN dotnet publish "CafeteriaApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final / Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CafeteriaApi.dll"]