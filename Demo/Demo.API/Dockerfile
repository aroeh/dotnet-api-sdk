FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Demo/Demo.API/Demo.API.csproj", "Demo/Demo.API/"]
COPY ["Demo/Demo.API.Core/Demo.API.Core.csproj", "Demo/Demo.API.Core/"]
COPY ["Demo/Demo.API.Infrastructure.MongoDb/Demo.API.Infrastructure.MongoDb.csproj", "Demo/Demo.API.Infrastructure.MongoDb/"]
COPY ["Demo/Demo.API.Shared/Demo.API.Shared.csproj", "Demo/Demo.API.Shared/"]
COPY ["Demo/Demo.API.Models/Demo.API.Models.csproj", "Demo/Demo.API.Models/"]

# Copy the SDK project files
COPY ["Dotnet.API.MongoDb.SDK/Dotnet.API.MongoDb.SDK.csproj", "Dotnet.API.MongoDb.SDK/"]
COPY ["Dotnet.API.Redis.SDK/Dotnet.API.Redis.SDK.csproj", "Dotnet.API.Redis.SDK/"]
COPY ["Dotnet.API.SDK/Dotnet.API.SDK.csproj", "Dotnet.API.SDK/"]
COPY ["Dotnet.API.SDK.Models/Dotnet.API.SDK.Models.csproj", "Dotnet.API.SDK.Models/"]

RUN dotnet restore "Demo/Demo.API/Demo.API.csproj"

COPY . .
WORKDIR "/src/Demo/Demo.API"
RUN dotnet build "Demo.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Demo.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Demo.API.dll"]
