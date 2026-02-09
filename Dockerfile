# ---------------------------
# STAGE 1: Build
# ---------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj и восстанавливаем зависимости
COPY OsuPngService.csproj .
RUN dotnet restore

# Копируем весь код
COPY . .

# Публикуем приложение
RUN dotnet publish -c Release -o /app/publish

# Скачиваем Chromium для Puppeteer
RUN dotnet tool install --global dotnet-script && \
    export PATH="$PATH:/root/.dotnet/tools" && \
    cd /app/publish && \
    dotnet exec /app/publish/OsuPngService.dll download-chromium || true

# ---------------------------
# STAGE 2: Runtime
# ---------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0

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
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Копируем опубликованное приложение
COPY --from=build /app/publish .

# Копируем Chromium из build стадии
COPY --from=build /root/.local-chromium /root/.local-chromium

# Открываем порт
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "OsuPngService.dll"]