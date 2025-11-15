# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the csproj first (layer caching)
COPY TaskManagement.API/*.csproj ./TaskManagement.API/
COPY TaskManagement.Application/*.csproj ./TaskManagement.Application/
COPY TaskManagement.Common/*.csproj ./TaskManagement.Common/
COPY TaskManagement.Domain/*.csproj ./TaskManagement.Domain/
COPY TaskManagement.Persistance/*.csproj ./TaskManagement.Persistance/

RUN dotnet restore TaskManagement.API/TaskManagement.API.csproj

# Copy everything now
COPY . .

# Publish
RUN dotnet publish TaskManagement.API/TaskManagement.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Railway sets PORT environment variable automatically
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "TaskManagement.API.dll"]
