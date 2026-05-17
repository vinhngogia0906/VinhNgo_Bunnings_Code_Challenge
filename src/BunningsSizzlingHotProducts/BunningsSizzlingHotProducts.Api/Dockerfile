# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /src
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy csprojs first for layer caching
COPY ["src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.Domain/BunningsSizzlingHotProducts.Domain.csproj",                 "BunningsSizzlingHotProducts.Domain/"]
COPY ["src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.Application/BunningsSizzlingHotProducts.Application.csproj",       "BunningsSizzlingHotProducts.Application/"]
COPY ["src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.Infrastructure/BunningsSizzlingHotProducts.Infrastructure.csproj", "BunningsSizzlingHotProducts.Infrastructure/"]
COPY ["src/BunningsSizzlingHotProducts/BunningsSizzlingHotProducts.Api/BunningsSizzlingHotProducts.Api.csproj",                       "BunningsSizzlingHotProducts.Api/"]

RUN dotnet restore "./BunningsSizzlingHotProducts.Api/BunningsSizzlingHotProducts.Api.csproj"

ARG BUILD_CONFIGURATION=Release
# Copy everything else and build the service project
COPY src/BunningsSizzlingHotProducts/ ./
RUN dotnet build "./BunningsSizzlingHotProducts.slnx" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BunningsSizzlingHotProducts.Api/BunningsSizzlingHotProducts.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BunningsSizzlingHotProducts.Api.dll"]