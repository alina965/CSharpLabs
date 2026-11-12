# Мастер прогулов

## Лабораторные

- **1 — консоль:** один семестр до 100 дней, итоговое и среднее удовольствие.
- **2 — Generic Host:** день каждые 5 секунд, DI, остановка клавишей или при отчислении.
- **3 — unit-тесты:** основные сценарии правил, исходов пары, истории и отчисления. Набор сокращён; все требования задания к тестам стратегии он не покрывает.
- **4 — PostgreSQL:** сохранение каждого дня, новый семестр и продолжение, четыре интеграционных теста.
- **5–6 — HTTP и очереди:** пока не реализованы.

Требуется .NET 10 SDK; для лабораторной 4 — PostgreSQL (ниже запуск через Docker Desktop).
Стратегия всегда посещает занятия: за 100 дней получает 100 удовольствия от пирожков. Конкурсная стратегия не реализована.

## Запуск приложения

Команды выполняются в PowerShell из корня проекта.

```powershell
# Лаба 1: один семестр без БД
dotnet run --project laba -- --console

# Лаба 2: непрерывная работа без БД
dotnet run --project laba
```

Для лабы 4 открой Docker Desktop. При первой настройке создай контейнер:

```powershell
docker run --name laba-postgres -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=laba_password -e POSTGRES_DB=laba -p 127.0.0.1:5432:5432 -v laba_pgdata:/var/lib/postgresql/data -d postgres:17
```

Если контейнер уже создан, запускай его командой `docker start laba-postgres`.
Проверь готовность: `docker exec laba-postgres pg_isready -U postgres -d laba` — ожидается `accepting connections`.

```powershell
# Подключение нужно задавать в каждом новом терминале
$env:LABA_DB = "Host=localhost;Port=5432;Database=laba;Username=postgres;Password=laba_password"

# Новый семестр: сохрани напечатанный идентификатор
dotnet run --project laba -- --db-new

# Продолжение: замени ID на идентификатор своего семестра
dotnet run --project laba -- --db-resume ID
```

Таблица создаётся автоматически. Режим с БД завершается после 100 дней или отчисления; завершённый семестр продолжить нельзя.
В режимах Host остановка — любая клавиша в интерактивной консоли или Ctrl+C. При перенаправленном вводе чтение клавиш отключено.
После остановки продолжение начинается со следующего дня после последнего сохранённого.

## Тесты

```powershell
# Только unit-тесты: PostgreSQL не нужен
dotnet test laba.Tests/laba.Tests.csproj --filter "FullyQualifiedName!~PostgresTests"
```

Для интеграционных тестов нужен работающий контейнер. Создай отдельную базу один раз (если она уже существует, пропусти команду):

```powershell
docker exec laba-postgres psql -U postgres -d postgres -c "CREATE DATABASE laba_tests;"
$env:LABA_TEST_DB = "Host=localhost;Port=5432;Database=laba_tests;Username=postgres;Password=laba_password"

# Только четыре интеграционных теста
dotnet test laba.Tests/laba.Tests.csproj --filter "FullyQualifiedName~PostgresTests"

# Все тесты: LABA_TEST_DB также должна быть задана
dotnet test laba.Tests/laba.Tests.csproj
```

Тесты удаляют собственные записи после выполнения. Данные Docker сохраняются в томе `laba_pgdata`.
