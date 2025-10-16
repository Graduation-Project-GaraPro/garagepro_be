# =======================
# BUILD STAGE
# =======================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file solution và tất cả các file csproj để tận dụng cache restore
COPY garagepro_backend.sln ./
COPY Garage_pro_api/*.csproj Garage_pro_api/
COPY BusinessObject/*.csproj BusinessObject/
COPY DataAccessLayer/*.csproj DataAccessLayer/
COPY Dtos/*.csproj Dtos/
COPY Repositories/*.csproj Repositories/
COPY Services/*.csproj Services/

# Restore dependencies
RUN dotnet restore "garagepro_backend.sln"

# Copy toàn bộ source code
COPY . .

# Build và publish
RUN dotnet publish "Garage_pro_api/Garage_pro_api.csproj" -c Release -o /app/publish

# =======================
# RUNTIME STAGE
# =======================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Garage_pro_api.dll"]
