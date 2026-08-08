# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project file first for layer-cached restore
COPY SkillBridgeTutors.API/SkillBridgeTutors.API.csproj SkillBridgeTutors.API/
RUN dotnet restore SkillBridgeTutors.API/SkillBridgeTutors.API.csproj

# Copy the rest of the source
COPY . .

# Publish in Release mode
RUN dotnet publish SkillBridgeTutors.API/SkillBridgeTutors.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create a non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

# Render uses port 10000 by default
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "SkillBridgeTutors.API.dll"]
