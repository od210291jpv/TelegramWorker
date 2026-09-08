# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first to leverage Docker layer caching for restore
COPY LocalLlmClient/LocalLlmClient.csproj LocalLlmClient/
COPY LlmApi/LlmApi.csproj LlmApi/

RUN dotnet restore LlmApi/LlmApi.csproj

# Copy all source files
COPY LocalLlmClient/ LocalLlmClient/
COPY LlmApi/ LlmApi/

# Publish the API
RUN dotnet publish LlmApi/LlmApi.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# ASP.NET Core listens on port 8080 by default in .NET 8+
EXPOSE 8080

ENTRYPOINT ["dotnet", "LlmApi.dll"]
