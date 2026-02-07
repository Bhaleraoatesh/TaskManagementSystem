# ================================
#   BUILD STAGE (.NET 8 SDK)
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files for each project
COPY TaskManagement.API/*.csproj TaskManagement.API/
COPY TaskManagement.Application/*.csproj TaskManagement.Application/
COPY TaskManagement.Common/*.csproj TaskManagement.Common/
COPY TaskManagement.Domain/*.csproj TaskManagement.Domain/
COPY TaskManagement.Persistance/*.csproj TaskManagement.Persistance/

RUN dotnet restore TaskManagement.API/TaskManagement.API.csproj

COPY . .

RUN dotnet publish TaskManagement.API/TaskManagement.API.csproj -c Release -o /app/publish

# ================================
#   RUNTIME STAGE (.NET 8)
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "TaskManagement.API.dll"]
