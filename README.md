# Домашняя библиотека (TestHomeLibrary)

Веб-приложение учёта домашней библиотеки: карточка и список книг, CRUD через хранимые процедуры SQL Server, оглавление книги в XML-поле через HTML-редактор (CKEditor 5), поиск по названию/автору/тексту оглавления.

## Стек

- .NET 10 (LTS), ASP.NET Core MVC + Razor
- SQL Server 2022 в Docker
- Dapper (хранимые процедуры)
- HTML-редактор: CKEditor 5 (CDN), оглавление хранится как XML в `xml`-колонке
- Unit-тесты: xUnit + Moq

## Архитектура (Clean Architecture)

```
src/TestHomeLibrary.Domain          — агрегат Book, value objects, интерфейс IBookRepository
src/TestHomeLibrary.Application     — BookService, DTO, ITableOfContentsConverter
src/TestHomeLibrary.Infrastructure  — DapperBookRepository, DatabaseInitializer, HtmlTableOfContentsConverter
src/TestHomeLibrary.Web             — MVC-контроллеры и представления
tests/TestHomeLibrary.UnitTests     — unit-тесты (43)
db/                                 — 01_Schema.sql, 02_StoredProcedures.sql, 03_Seed.sql
```

Схема и хранимые процедуры применяются автоматически при старте приложения (`DatabaseInitializer`), сид-данные входят в `03_Seed.sql`.

## Запуск

Требования: Docker, .NET 10 SDK.

```powershell
# 1. SQL Server
docker compose up -d

# 2. Приложение (из корня решения)
dotnet run --project src/TestHomeLibrary.Web
```

Откроется http://localhost:5248 (профиль запуска `http`).

Схема БД, процедуры и тестовые данные создаются автоматически при первом старте (ретраи, пока SQL Server не готов). Строка подключения — `src/TestHomeLibrary.Web/appsettings.json`.

## Тесты

```powershell
dotnet test TestHomeLibrary.slnx
```

## Основные сценарии

- Список книг: сортировка (название/автор/год), постраничный вывод, поиск (`?q=`) по названию, автору и тексту оглавления.
- Создание/редактирование: поля карточки + HTML-оглавление (CKEditor); HTML конвертируется в XML при сохранении и обратно при отображении.
- Удаление: подтверждение, затем редирект в список; несуществующая книга — 404.
