# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaura primero usando solo los .csproj para aprovechar el cache de capas.
COPY src/EventosVivos.Api/*.csproj src/EventosVivos.Api/
COPY src/EventosVivos.Application/*.csproj src/EventosVivos.Application/
COPY src/EventosVivos.Domain/*.csproj src/EventosVivos.Domain/
COPY src/EventosVivos.Infrastructure/*.csproj src/EventosVivos.Infrastructure/
RUN dotnet restore src/EventosVivos.Api/EventosVivos.Api.csproj

# Copia el resto del código y publica.
COPY . .
RUN dotnet publish src/EventosVivos.Api/EventosVivos.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "EventosVivos.Api.dll"]
