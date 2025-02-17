FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OCMS_WebAPI/OCMS_WebAPI.csproj", "OCMS_WebAPI/"]
COPY ["OCMS_BOs/OCMS_BOs.csproj", "OCMS_BOs/"]
COPY ["OCMS_Repositories/OCMS_Repositories.csproj", "OCMS_Repositories/"]
COPY ["OCMS_Services/OCMS_Services.csproj", "OCMS_Services/"]
RUN dotnet restore "./OCMS_WebAPI/OCMS_WebAPI.csproj"
COPY . .
WORKDIR "/src/OCMS_WebAPI"
RUN dotnet build "./OCMS_WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN ls -la /src/OCMS_WebAPI   # Debugging: Verify files are in place
RUN dotnet publish "/src/OCMS_WebAPI/OCMS_WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OCMS_WebAPI.dll"]
