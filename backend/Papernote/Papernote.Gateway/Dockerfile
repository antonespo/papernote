FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

RUN adduser --disabled-password --home /app --gecos '' appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["Papernote.Gateway/Papernote.Gateway.csproj", "Papernote.Gateway/"]
COPY ["Papernote.SharedMicroservices/Papernote.SharedMicroservices.csproj", "Papernote.SharedMicroservices/"]
COPY ["Directory.Build.props", "./"]

RUN dotnet restore "Papernote.Gateway/Papernote.Gateway.csproj"

COPY . .

WORKDIR "/src/Papernote.Gateway"
RUN dotnet publish "Papernote.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Papernote.Gateway.dll"]