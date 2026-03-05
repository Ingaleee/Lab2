FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/OrderTracking.Presentation.Api/OrderTracking.Presentation.Api.csproj", "src/OrderTracking.Presentation.Api/"]
COPY ["src/OrderTracking.Application/OrderTracking.Application.csproj", "src/OrderTracking.Application/"]
COPY ["src/OrderTracking.Infrastructure/OrderTracking.Infrastructure.csproj", "src/OrderTracking.Infrastructure/"]
COPY ["src/OrderTracking.Domain/OrderTracking.Domain.csproj", "src/OrderTracking.Domain/"]
COPY ["src/OrderTracking.Contracts/OrderTracking.Contracts.csproj", "src/OrderTracking.Contracts/"]
RUN dotnet restore "src/OrderTracking.Presentation.Api/OrderTracking.Presentation.Api.csproj"
COPY . .
WORKDIR "/src/src/OrderTracking.Presentation.Api"
RUN dotnet build "OrderTracking.Presentation.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderTracking.Presentation.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderTracking.Presentation.Api.dll"]
