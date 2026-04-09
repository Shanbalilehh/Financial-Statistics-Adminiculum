# BUILD: Get microsoft sdk 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore dependencies
COPY FunctionGemma.Api.csproj .
RUN dotnet restore

# Copy all project and publish
COPY models /app/models
COPY . .
RUN dotnet publish -c release -o /app --no-restore

# RUNTIME: Get microsoft runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "FunctionGemma.Api.dll"]