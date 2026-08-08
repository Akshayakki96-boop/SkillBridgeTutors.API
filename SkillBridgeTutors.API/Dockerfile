# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SkillBridgeTutors.API.csproj", "./"]
RUN dotnet restore "SkillBridgeTutors.API.csproj"

COPY . .
RUN dotnet publish "SkillBridgeTutors.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "SkillBridgeTutors.API.dll"]