FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Wolfremium.Estimados.WebApp/Wolfremium.Estimados.WebApp.csproj", "Wolfremium.Estimados.WebApp/"]
RUN dotnet restore "Wolfremium.Estimados.WebApp/Wolfremium.Estimados.WebApp.csproj"
COPY . .
WORKDIR "/src/Wolfremium.Estimados.WebApp"
RUN dotnet publish "Wolfremium.Estimados.WebApp.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY Wolfremium.Estimados.WebApp/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
