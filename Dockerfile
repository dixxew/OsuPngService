# ---------------------------
# STAGE 1: Build
# ---------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем csproj и восстанавливаем зависимости
COPY OsuPngService/OsuPngService.csproj OsuPngService/
RUN dotnet restore OsuPngService/OsuPngService.csproj

# Копируем весь код
COPY OsuPngService/ OsuPngService/

# Публикуем приложение
WORKDIR /src/OsuPngService
RUN dotnet publish -c Release -o /app/publish

# ---------------------------
# STAGE 2: Runtime
# ---------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Зависимости для Chromium
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    ca-certificates \
    fonts-liberation \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    libgbm1 \
    libasound2 \
    libpango-1.0-0 \
    libcairo2 \
    libappindicator3-1 \
    xdg-utils \
    curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Копируем опубликованное приложение
COPY --from=build /app/publish .
# Открываем порт
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "OsuPngService.dll"]