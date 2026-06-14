FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Wolfremium.Estimados.Api/Wolfremium.Estimados.Api.csproj", "Wolfremium.Estimados.Api/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "Wolfremium.Estimados.Api/Wolfremium.Estimados.Api.csproj"
COPY . .
WORKDIR "/src/Wolfremium.Estimados.Api"
RUN dotnet build "Wolfremium.Estimados.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Wolfremium.Estimados.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wolfremium.Estimados.Api.dll"]
