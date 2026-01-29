# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy central package management files
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

# Copy solution file
COPY ["PadTime.sln", "./"]

# Copy all project files
COPY ["src/PadTime.API/PadTime.API.csproj", "src/PadTime.API/"]
COPY ["src/PadTime.Application/PadTime.Application.csproj", "src/PadTime.Application/"]
COPY ["src/PadTime.Domain/PadTime.Domain.csproj", "src/PadTime.Domain/"]
COPY ["src/PadTime.Infrastructure/PadTime.Infrastructure.csproj", "src/PadTime.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/PadTime.API/PadTime.API.csproj"

# Copy all source code
COPY . .

# Build
WORKDIR "/src/src/PadTime.API"
RUN dotnet build "PadTime.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "PadTime.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PadTime.API.dll"]
