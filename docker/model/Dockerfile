# Standalone Containerized FunctionGemma Inference Microservice
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FunctionGemma.Api/FunctionGemma.Api.csproj ./FunctionGemma.Api/
COPY Shared/Shared.csproj ./Shared/
RUN dotnet restore FunctionGemma.Api/FunctionGemma.Api.csproj

COPY FunctionGemma.Api/ ./FunctionGemma.Api/
COPY Shared/ ./Shared/
RUN dotnet publish FunctionGemma.Api/FunctionGemma.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV Paths__modelPath=/models/functiongemma_oga

HEALTHCHECK --interval=10s --timeout=5s --retries=3 CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "FunctionGemma.Api.dll"]
