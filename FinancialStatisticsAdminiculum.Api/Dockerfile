# Multi-stage build .NET
# Stage 1: Build App
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Stage 2: Copy and restore .csproj
COPY *.sln .
COPY FinancialStatisticsAdminiculum.Api/*.csproj ./FinancialStatisticsAdminiculum.Api/
COPY FinancialStatisticsAdminiculum.Infrastructure/*.csproj ./FinancialStatisticsAdminiculum.Infrastructure/
COPY FinancialStatisticsAdminiculum.Application/*.csproj ./FinancialStatisticsAdminiculum.Application/
COPY FinancialStatisticsAdminiculum.Core/*.csproj ./FinancialStatisticsAdminiculum.Core/
RUN dotnet restore

# Stage 3: Copy Model, copy everything else and build app
COPY . .
WORKDIR /src/FinancialStatisticsAdminiculum.Api
RUN dotnet publish -c release -o /app --no-restore

# Stage 4: Run App
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "FinancialStatisticsAdminiculum.Api.dll"] 

