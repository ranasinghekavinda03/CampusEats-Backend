
# ==========================================
# STAGE 1 - BUILD ASP.NET CORE API
# ==========================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project file and restore dependencies
COPY ["CampusEats.Api.csproj", "./"]
RUN dotnet restore "CampusEats.Api.csproj"

# Copy remaining application files
COPY . .

# Publish application in Release mode
RUN dotnet publish "CampusEats.Api.csproj" -c Release -o /app/publish --no-restore


# ==========================================
# STAGE 2 - RUN ASP.NET CORE API
# ==========================================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Configure production environment
ENV ASPNETCORE_ENVIRONMENT=Production

# Listen on Render-compatible port
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

EXPOSE 8080

# Start CampusEats API
ENTRYPOINT ["dotnet", "CampusEats.Api.dll"]
