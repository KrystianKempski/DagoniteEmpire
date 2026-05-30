FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "./"]
COPY ["DagoniteEmpire/DagoniteEmpire.csproj", "DagoniteEmpire/"]
COPY ["DA_Business/DA_Business.csproj", "DA_Business/"]
COPY ["DA_Models/DA_Models.csproj", "DA_Models/"]
COPY ["DA_DataAccess/DA_DataAccess.csproj", "DA_DataAccess/"]
COPY ["DA_Common/DA_Common.csproj", "DA_Common/"]
COPY ["DA_Scribe/DA_Scribe.csproj", "DA_Scribe/"]
RUN dotnet restore "./DagoniteEmpire/DagoniteEmpire.csproj"
COPY . .
WORKDIR "/src/DagoniteEmpire"
RUN dotnet build "./DagoniteEmpire.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DagoniteEmpire.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ARG ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT $ENVIRONMENT

# SCRIBE / Ollama configuration is read from appsettings + environment variables.
# When deploying the app on a server with the GPU host running Ollama elsewhere,
# override the base URL without rebuilding the image, e.g.:
#   docker run -e Scribe__Ollama__BaseUrl=http://gpu-host:11434 \
#              -e Scribe__Ollama__ChatModel=qwen2.5:14b \
#              -e Scribe__Ollama__TimeoutSeconds=180 \
#              ...
# .NET configuration provider maps "Scribe:Ollama:BaseUrl" -> "Scribe__Ollama__BaseUrl".

ENTRYPOINT ["dotnet", "DagoniteEmpire.dll"]
