FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DagoniteEmpire/DagoniteEmpire.csproj", "DagoniteEmpire/"]
COPY ["DA_Business/DA_Business.csproj", "DA_Business/"]
COPY ["DA_Models/DA_Models.csproj", "DA_Models/"]
COPY ["DA_DataAccess/DA_DataAccess.csproj", "DA_DataAccess/"]
COPY ["DA_Common/DA_Common.csproj", "DA_Common/"]
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

ENTRYPOINT ["dotnet", "DagoniteEmpire.dll"]
