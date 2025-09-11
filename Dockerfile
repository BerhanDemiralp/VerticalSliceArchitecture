# -----------------------
# Base (runtime)
# -----------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# -----------------------
# Build
# -----------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Restore için sadece csproj
COPY ["VerticalSliceArchitecture.csproj", "."]
RUN dotnet restore "./VerticalSliceArchitecture.csproj"

# Kaynaklarýn tamamý
COPY . .
# Git sorgularýný kapat (SourceLink hatasýný önler)
RUN dotnet build "./VerticalSliceArchitecture.csproj" \
    -c $BUILD_CONFIGURATION -o /app/build \
    -p:EnableSourceControlManagerQueries=false

# -----------------------
# Publish
# -----------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./VerticalSliceArchitecture.csproj" \
    -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=false \
    -p:EnableSourceControlManagerQueries=false

# -----------------------
# Final (runtime)
# -----------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "VerticalSliceArchitecture.dll"]
