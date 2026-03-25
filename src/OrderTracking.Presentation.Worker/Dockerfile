FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/OrderTracking.Presentation.Worker/OrderTracking.Presentation.Worker.csproj", "src/OrderTracking.Presentation.Worker/"]
COPY ["src/OrderTracking.Application/OrderTracking.Application.csproj", "src/OrderTracking.Application/"]
COPY ["src/OrderTracking.Infrastructure/OrderTracking.Infrastructure.csproj", "src/OrderTracking.Infrastructure/"]
COPY ["src/OrderTracking.Domain/OrderTracking.Domain.csproj", "src/OrderTracking.Domain/"]
COPY ["src/OrderTracking.Contracts/OrderTracking.Contracts.csproj", "src/OrderTracking.Contracts/"]
RUN dotnet restore "src/OrderTracking.Presentation.Worker/OrderTracking.Presentation.Worker.csproj"
COPY . .
WORKDIR "/src/src/OrderTracking.Presentation.Worker"
RUN dotnet build "OrderTracking.Presentation.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderTracking.Presentation.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderTracking.Presentation.Worker.dll"]
