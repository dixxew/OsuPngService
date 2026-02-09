# OsuPngService

Микросервис на **ASP.NET + PuppeteerSharp**, который рендерит HTML-шаблон и отдаёт **PNG-картинку** по HTTP.
Используется Chromium в headless-режиме. Подходит для генерации превьюшек / баннеров / карточек (osu! moment).

---

## Что делает

* Поднимает HTTP-сервис
* По `GET /osupng`:

    * берёт HTML-шаблон
    * вшивает картинки через base64
    * рендерит всё это в Chromium
    * возвращает готовый PNG

Без параметров, без стейта, тупо “дай картинку” 🎨

---

## Стек

* .NET (minimal API)
* PuppeteerSharp
* Chromium (скачивается автоматически)
* Docker / docker-compose
* Jenkins (CI-ready)

---

## Эндпоинты

### `GET /osupng`

Возвращает:

* `Content-Type: image/png`
* тело — PNG-картинка

Пример:

```bash
curl http://localhost:5000/osupng --output result.png
```

---

## Шаблон

Шаблон лежит тут:

```
OsuPngService/Templates/osupng/template.html
```

Ассеты:

```
OsuPngService/Templates/osupng/*.png
```

В HTML используются плейсхолдеры вида:

```html
<img src="{{osu_logo}}" />
```

Они заменяются на `data:image/png;base64,...` перед рендером.

---

## Как запустить локально

### Без докера

```bash
dotnet restore
dotnet run
```

Сервис стартанёт и будет слушать стандартный порт ASP.NET.

---

### Через Docker

```bash
docker build -t osupngservice .
docker run -p 5000:8080 osupngservice
```

или

```bash
docker-compose up --build
```

---

## Важные моменты

* Chromium **скачивается при первом запуске**
* Используется **один браузер на всё приложение** (через `SemaphoreSlim`)
* Без `--no-sandbox` в докере оно сдохнет — флаги уже проставлены
* Шаблон и ассеты должны **копироваться в output** (проверь `.csproj`, если будешь двигать файлы)

---

## Структура проекта

```
OsuPngService/
 ├─ Program.cs
 ├─ Templates/
 │   └─ osupng/
 │       ├─ template.html
 │       └─ *.png
 ├─ Dockerfile
 ├─ docker-compose.yml
 ├─ Jenkinsfile
 └─ OsuPngService.sln
```

---

## Что можно улучшить (если руки дойдут)

* Параметры в query (ник, аватар, текст и т.п.)
* Чекнуть что там с lifecycle браузера
* Кеширование результата
* Пул страниц вместо `NewPageAsync` каждый раз
* Таймауты и нормальный error handling
