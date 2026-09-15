# BUILD: Get microsoft sdk 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore dependencies
COPY FunctionGemma.Api/FunctionGemma.Api.csproj ./FunctionGemma.Api/
COPY Shared/Shared.csproj ./Shared/
RUN dotnet restore FunctionGemma.Api/FunctionGemma.Api.csproj

# Copy all project and publish
COPY FunctionGemma.Api/models /app/models/
COPY FunctionGemma.Api/ ./FunctionGemma.Api/
COPY Shared/ ./Shared/
RUN dotnet publish FunctionGemma.Api/FunctionGemma.Api.csproj -c release -o /app --no-restore

# RUNTIME: Get microsoft runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
ENTRYPOINT ["dotnet", "FunctionGemma.Api.dll"]