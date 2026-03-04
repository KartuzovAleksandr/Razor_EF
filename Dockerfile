# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Копируем сертификат в образ
COPY ["ASP_Docker.pfx", "/https/ASP_Docker.pfx"]

# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Razor_EF/Razor_EF.csproj", "Razor_EF/"]
RUN dotnet restore "./Razor_EF/Razor_EF.csproj"
COPY . .
WORKDIR "/src/Razor_EF"
RUN dotnet build "./Razor_EF.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Razor_EF.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# еще одно копирование в выходной каталог
COPY --from=base /https/ASP_Docker.pfx /https/ASP_Docker.pfx
ENTRYPOINT ["dotnet", "Razor_EF.dll"]