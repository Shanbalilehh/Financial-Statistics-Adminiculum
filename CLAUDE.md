# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a financial statistics analysis system built with .NET 8.0, featuring:
- AI-powered financial analysis with tool calling capabilities
- PostgreSQL database for data persistence
- Dockerized deployment using a multi-service compose setup
- Integration with ONNX Runtime GenAI for AI inference
- OpenTelemetry for observability
- Serilog for structured logging

## Architecture

The system follows a layered architecture pattern with clear separation of concerns:

1. **API Layer** (`FinancialStatisticsAdminiculum.Api`)
   - ASP.NET Core Web API with controllers
   - Dependency injection setup
   - Middleware for logging and exception handling
   - Swagger/OpenAPI documentation

2. **Application Layer** (`FinancialStatisticsAdminiculum.Application`)
   - Business logic and orchestration
   - Services for AI analysis and trend analysis
   - Interfaces for DI
   - AI-related components including tool resolvers and schema aggregators

3. **Core Layer** (`FinancialStatisticsAdminiculum.Core`)
   - Domain entities and value objects
   - Core interfaces and abstractions
   - Exception handling components

4. **Infrastructure Layer** (`FinancialStatisticsAdminiculum.Infrastructure`)
   - Data access with Entity Framework Core
   - Database context and repositories
   - ONNX runtime integration for AI inference
   - Message publishing infrastructure

5. **FunctionGemma.Api** (Separate service)
   - Dedicated AI inference service
   - ONNX Runtime GenAI integration
   - Health checks and observability

## Key Technical Details

- **Database**: PostgreSQL (using Npgsql.EntityFrameworkCore.PostgreSQL)
- **AI Inference**: ONNX Runtime GenAI integration
- **Observability**: OpenTelemetry with traces and metrics
- **Logging**: Serilog with structured logging
- **Dependency Injection**: ASP.NET Core built-in DI container
- **Security**: Castle DynamicProxy for interceptors
- **Messaging**: RabbitMQ for communication between services

## Development Setup

### Build and Run
```bash
# Build the solution
dotnet build

# Run the API
dotnet run --project FinancialStatisticsAdminiculum.Api

# Run in development mode with Docker
docker-compose up
```

### Testing
- Tests are likely in a separate test project
- Use `dotnet test` to run tests
- Unit tests for business logic, integration tests for API endpoints

### Database Migrations
- Database migrations are handled via EF Core
- Run `dotnet ef migrations add <MigrationName>` to create new migrations
- Run `dotnet ef database update` to apply migrations

## Common Development Tasks

1. **Adding new AI tools**: Create a new tool handler class implementing `IGemmaTool` and register it in Program.cs
2. **Adding new entities**: Create a new class in `FinancialStatisticsAdminiculum.Core.Entities` and update `AppDbContext`
3. **Creating new API endpoints**: Add a controller in `FinancialStatisticsAdminiculum.Api/Controllers`
4. **Modifying database schema**: Create and apply EF Core migrations
5. **Adding new services**: Register in `Program.cs` with appropriate lifetime (Scoped/Singleton)

## Notable Patterns and Practices

- Custom Generic Repository pattern for data access
- Custom Unit of Work pattern for transaction management
- Castle DynamicProxy for interception (security, logging, etc.)
- Tool calling architecture with dynamic schema generation
- Dependency injection with keyed services for different tool types
- Structured logging with Serilog
- OpenTelemetry for distributed tracing
- Health checks for service monitoring