# ================================
#   BUILD STAGE (DOTNET 8 SDK)
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first (improves caching)
COPY TaskManagement.API/*.csproj TaskManagement.API/
COPY TaskManagement.Application/*.csproj TaskManagement.Application/
COPY TaskManagement.Common/*.csproj TaskManagement.Common/
COPY TaskManagement.Domain/*.csproj TaskManagement.Domain/
COPY TaskManagement.Persistance/*.csproj TaskManagement.Persistance/

# Restore dependencies
RUN dotnet restore TaskManagement.API/TaskManagement.API.csproj

# Copy entire solution
COPY . .

# Publish the app
RUN dotnet publish TaskManagement.API/TaskManagement.API.csproj -c Release -o /app/publish

# ================================
#   RUNTIME STAGE (DOTNET 8 ASP.NET)
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Railway injects PORT automatically
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "TaskManagement.API.dll"]
