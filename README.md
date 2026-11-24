# CrossApp

Наскрізний проєкт з крос-платформного програмування.

## Предметна область

Склад.

### Сутності

- Product — товар
- StockBatch — партія товару
- Warehouse — склад
- Movement — переміщення товару

### Призначення

Застосунок призначений для обліку залишків товарів по партіях на складі.

## Запуск

```bash
dotnet build
dotnet run --project src/Cli
```
## Додатковий режим виводу (JSON)

```bash
dotnet run --project src/Cli -- --json
```

## Середовище 

.NET SDK 10.0, Windows 11 x64

## Порівняння self-contained публікацій
- win-x64: ~76,6 MB
- linux-x64: ~78,7 MB

## Порівняння OSDescription: локально vs Docker

- Локально (Windows): Microsoft Windows 10.0.26200
- У Docker-контейнері (Linux): Ubuntu 24.04.4 LTS

## Структура solution

```
CrossApp/
├── CrossApp.sln
├── README.md
├── .gitignore
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   └── EnvironmentInfo.cs
    └── Cli/
        ├── Cli.csproj
        └── Program.cs
```

Проєкт `Core` — бібліотека класів (classlib), не має точки входу.
Проєкт `Cli` — консольний застосунок, посилається на `Core` через `ProjectReference`.
Напрямок залежності односторонній: **Cli → Core**.

## Команди

**Додавання Core та посилання:**
```
dotnet new classlib -n Core -o src/Core -f net10.0
dotnet sln add src/Core/Core.csproj
dotnet add src/Cli/Cli.csproj reference src/Core/Core.csproj
```

**Збірка та запуск:**
```
dotnet build
dotnet run --project src/Cli
```

**Публікація (self-contained / framework-dependent, Windows та Linux):**
```
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -o publish-self-contained
dotnet publish src/Cli -c Release -r win-x64 --self-contained false -o publish-framework-dependent
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true -o publish-linux-self-contained
```

## Self-contained vs framework-dependent

**Self-contained** публікація включає код застосунку, всі залежності NuGet і копію .NET runtime для вказаного RID. Застосунок можна запускати на машині без встановленого .NET, але каталог значно більший і прив'язаний до конкретної платформи (RID).

**Framework-dependent** публікація містить лише код застосунку і його залежності, без runtime. Каталог набагато менший, але на машині користувача має бути встановлений сумісний .NET Runtime відповідної версії.

## Порівняння режимів публікації

| RID | Режим | Розмір publish | Потрібен встановлений runtime |
|---|---|---|---|
| win-x64 | self-contained | ~76,68 МБ | ні |
| win-x64 | framework-dependent | ~0,19 МБ | так (.NET 10) |
| linux-x64 | self-contained | ~78,80 МБ | ні |

## Структура Core (домовленість на семестр)

```
Core/
├── Dto/      – record-типи формату даних (з'явиться на тижні 3)
├── Domain/   – сутності з поведінкою та інваріантами (з тижня 4)
└── Storage/  – реалізації сховищ (з тижня 5)
```

## Команди запуску

### Склад сувенірної продукції — імпорт з CSV (за замовчуванням)
```bash
dotnet run --project src/Cli
```
Без аргументів використовується файл `data/sample.csv`.

### Явний шлях до CSV-файлу з коректними даними
```bash
dotnet run --project src/Cli data/sample_clean.csv
```

### CSV-файл із навмисно пошкодженими рядками
```bash
dotnet run --project src/Cli data/sample.csv
```

### Неіснуючий файл (перевірка обробки помилки)
```bash
dotnet run --project src/Cli data/no_such.csv
```

### Імпорт з JSON
```bash
dotnet run --project src/Cli data/sample.json
```

### Змішаний файл (товари + постачальники за префіксом)
```bash
dotnet run --project src/Cli data/mixed.csv
```

## Інваріанти домену (lab04)

**Product**
- Id, SKU, назва, одиниця виміру, категорія не порожні — ArgumentException
- Категорія з дозволеного списку — ArgumentException
- Початковий залишок ≥ 0 — ArgumentOutOfRangeException
- Кількість приходу та видачі > 0 — ArgumentOutOfRangeException
- Не можна видати більше, ніж залишок — InvalidOperationException
- Id і SKU незмінні, SKU нормалізується до верхнього регістру

**IssueRequest**
- Номер, товар, одержувач обов'язкові — ArgumentException / ArgumentNullException
- Кількість у заявці > 0 — ArgumentOutOfRangeException
- Переходи тільки Draft→Confirmed та Draft→Cancelled — InvalidOperationException
- Підтвердження при нестачі товару — InvalidOperationException

**IssuePolicy**
- Не більше 5 відкритих заявок на одержувача — InvalidOperationException

FromDto проходить ті самі перевірки, що й Create.

## Lab 05: сервісний шар і ручний DI

- Інтерфейс: `ICatalogStore` (List, GetById, Add, Update, Remove)
- Реалізації: `InMemoryCatalogStore`, `FileCatalogStore` (+ декоратор `CachingCatalogStore`)
- Сервіс: `CatalogService` (Add, Receive, Issue, All, Find, Search)
- Composition root: `src/Cli/Program.cs`

dotnet run --project src/Cli                  # у пам'яті
dotnet run --project src/Cli -- --file        # файл catalog.json
dotnet run --project src/Cli -- --file --cache
dotnet run --project src/Cli data/sample.csv  # старий імпорт з lab03/04

## Lab 06: доменні події, Action/Func, лямбди

- Події: `CatalogChangeKind` (Added, Received, Issued, Removed), `CatalogChangedEventArgs` (Id, Kind, Delta, QuantityAfter, At)
- `CatalogService.Changed` (`EventHandler<CatalogChangedEventArgs>`), підняття лише після успішної зміни
- Підписники в Cli: локальний метод (консоль + `logs/app.log`) і лямбда-лічильник
- `FindAll(Func<Product,bool>)`; додатково: `INotificationSink` (ConsoleSink, FileSink), `Action<string>` в імпорті

dotnet run --project src/Cli

## Lab 07: LINQ-звіти

Усі команди виконуються з кореня репозиторію. Назву звіту передаємо першим аргументом без `--`, тому прапорці `--file` і `--cache` її не підміняють.

### Основні звіти

```bash
dotnet run --project src/Cli -- report-top
```
Топ-5 позицій за залишком (`OrderByDescending` + `ThenBy` за SKU + `Take`). Другий ключ робить порядок однаковим між запусками.

```bash
dotnet run --project src/Cli -- report-low
```
Дефіцит: позиції із залишком менше 10, від меншого до більшого (`Where` + `OrderBy` + `Select` у DTO).

```bash
dotnet run --project src/Cli -- report-groups
```
Групування за одиницею виміру: кількість позицій і сумарний залишок у кожній групі (`GroupBy` + `Count` + `Sum`).

```bash
dotnet run --project src/Cli -- report-sum
```
Підсумки: кількість позицій, загальний залишок, середнє (`double`) і максимум.

### Перевірки

```bash
dotnet run --project src/Cli -- report-empty
```
Усі звіти на порожньому джерелі: виводить `(порожньо)` і нулі, без винятків.

```bash
dotnet run --project src/Cli -- report-lazy
```
Демо відкладеного виконання: `Count()` до зміни джерела, `Count()` після зміни (число інше), потім `ToList()` фіксує знімок, який не змінюється після `Clear()`.

### Додаткові завдання

```bash
dotnet run --project src/Cli -- report-top-groups
```
Топ-2 одиниці за сумарним залишком. Використовує власний метод-розширення `TopBy` (другий звіт, де він застосований).

```bash
dotnet run --project src/Cli -- report-groups-query
```
Те саме групування, але в синтаксисі запитів (`from ... group ... into`). Виводить також перевірку, що результат збігається з методним синтаксисом.

```bash
dotnet run --project src/Cli -- report-moves
```
`Join` товарів і переміщень: лише ті товари, які мають рухи. Рух по неіснуючому товару відкидається.

```bash
dotnet run --project src/Cli -- report-moves-sum
```
`GroupJoin`: усі товари, навіть без переміщень, з кількістю рухів і сальдо.

```bash
dotnet run -c Release --project src/Cli -- report-bench
```
Stopwatch на 100 000 згенерованих записів: порівняння запиту, що перебирається тричі, з одноразовим `ToList()`, а також топ-N з `ToList()` посередині ланцюжка і без нього. Запускати в `Release`, щоб час був показовим.

### Скидання стану

Звіти працюють із `SampleData` у пам'яті, тому числа однакові за кожного запуску. Для демо подій попередньої роботи тепер окрема команда `demo-events`.

## Lab 08

### Тести

```bash
dotnet test
dotnet test --collect:"XPlat Code Coverage"
```

### Перевірка імпорту та скасування

```bash
dotnet run --project src/Cli -- import
dotnet run --project src/Cli -- import --file    # перший раз 17, другий раз 0 (ідемпотентність)
dotnet run --project src/Cli -- import-gen --count=17 --path=data/import.json
dotnet run --project src/Cli -- import --path=data/big.json --timeout=30
```

- `Ctrl+C` під час `import --path=data/big.json` завершує запуск з повідомленням `Імпорт скасовано.`.
- `--stream` імпортує записи у потоці: `dotnet run --project src/Cli -- import --path=data/big.json --stream`.
- Для пошкодженого JSON: `dotnet run --project src/Cli -- import --path=data/sample.json` показує `Дані відхилено: Запис 9: SKU не може бути порожнім`.

### Інваріанти і винятки

- `Product.Issue`, `IssueRequest.EnsureTransition` і `IssuePolicy.EnsureRecipientCanOpen` тепер кидають `DomainException` замість `InvalidOperationException`.
- `CatalogService.Receive` і `CatalogService.Issue` також кидають `DomainException` для відсутнього запису.
- `StorageException` використовується для помилок читання/парсингу JSON, зберігаючи `InnerException`.

### Структура solution

`CrossApp.slnx` містить обидві проєктні групи, включно з `tests/Core.Tests`.

## Lab 09: шляхи, конфігурація, secrets

Джерела конфігурації (пізніше перекриває раніше):
`appsettings.json` → `appsettings.{CROSSAPP_ENVIRONMENT}.json` → user-secrets → змінні `CROSSAPP_*` → аргументи `--Key=value`.

Профіль за замовчуванням — Development. Відносні шляхи рахуються від `AppContext.BaseDirectory` (каталог збірки), а не від поточного каталогу, тому запуск з кореня solution і з `src/Cli` дає один і той самий файл даних.

### Профілі

| Профіль | Як увімкнути | Logging:MinimumLevel | Дані |
|---|---|---|---|
| Development | за замовчуванням | Debug | `<BaseDirectory>/data/items.json` |
| Production | `$env:CROSSAPP_ENVIRONMENT = "Production"` | Information | `<BaseDirectory>/data/items.json` |

### Таблиця ключів

| Ключ | Змінна середовища | За замовчуванням |
|---|---|---|
| Storage:DataPath | CROSSAPP_Storage__DataPath | data |
| Storage:FileName | CROSSAPP_Storage__FileName | items.json |
| Logging:MinimumLevel | CROSSAPP_Logging__MinimumLevel | Information (Debug у Development) |
| EnvironmentName | CROSSAPP_EnvironmentName | Production (у Development — Development) |
| Warehouse:DefaultUnit | CROSSAPP_Warehouse__DefaultUnit | шт |
| Integration:ApiKey | CROSSAPP_Integration__ApiKey | немає (secret) |
| — (простий шлях) | CROSSAPP_DATA_PATH | перекриває шлях з усіх джерел |
| — (профіль) | CROSSAPP_ENVIRONMENT | Development |

У назвах змінних двокрапка з ключа замінюється на `__`.

### PowerShell

```powershell
$env:CROSSAPP_DATA_PATH = "$env:TEMP\crossapp-data"
$env:CROSSAPP_Integration__ApiKey = "from-env"
$env:CROSSAPP_ENVIRONMENT = "Production"
dotnet run --project src/Cli -- export

# повернути стан (змінні живуть до кінця сесії)
Remove-Item Env:CROSSAPP_DATA_PATH, Env:CROSSAPP_Integration__ApiKey, Env:CROSSAPP_ENVIRONMENT
```

### bash (Linux/macOS)

```bash
CROSSAPP_DATA_PATH=/tmp/data dotnet run --project src/Cli -- export
CROSSAPP_Integration__ApiKey=from-env dotnet run --project src/Cli -- config
dotnet run --project src/Cli -- config --Logging:MinimumLevel=Warning
```

### Secrets

```powershell
dotnet user-secrets set "Integration:ApiKey" "..." --project src/Cli
dotnet user-secrets list --project src/Cli
```

`secrets.json` лежить поза репозиторієм: у Windows `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\`, у Linux/macOS `~/.microsoft/usersecrets/<UserSecretsId>/`. У git комітиться лише `UserSecretsId` у `Cli.csproj`. Консоль друкує тільки факт наявності ключа і його довжину.

## Lab 10: Minimal API (Склад)

`src/Api` — друга точка входу поряд із `src/Cli`. Обидва проєкти використовують один `Core`, який не залежить від HTTP. Cli і `Core.Tests` залишаються робочими.

**Порт:** `http://localhost:5080`

### Запуск

```bash
dotnet run --project src/Api --urls http://localhost:5080
```

Лише в Development:
- Scalar UI: http://localhost:5080/scalar/v1
- OpenAPI: http://localhost:5080/openapi/v1.json

### Маршрути

| Метод | Шлях | Тіло | Коди |
|---|---|---|---|
| GET | `/api/products?page=1&pageSize=20&search=...` | — | 200 `{items,total}`, 400 |
| GET | `/api/products/low-stock?threshold=5` | — | 200, 400 |
| GET | `/api/products/{id}` | — | 200, 404 |
| POST | `/api/products` | `SaveProductRequest` | 201 (Location), 400, 409 |
| PUT | `/api/products/{id}` | `SaveProductRequest` | 200, 400, 404, 409 |
| DELETE | `/api/products/{id}` | — | 204, 404 |
| GET | `/health` | — | 200 |

`SaveProductRequest`: `sku`, `name`, `category`, `unit`, `quantity`, `note` (необов'язкове).
`ProductDto`: `id`, `sku`, `name`, `category`, `unit`, `quantity`, `note`.

Помилки повертаються у форматі ProblemDetails (RFC 9457): 400 містить словник `errors` по полях, 409 (порушення бізнес-правила, наприклад дубль SKU) містить `detail`. Stack trace клієнту не показується.

### Приклади curl

Кирилицю передаємо через heredoc із `--data-binary`: `curl` у Git Bash під Windows надсилає аргумент `-d` не в UTF-8, і сервер відповідає 400.

```bash
# створити -> 201 + Location
curl -i -X POST http://localhost:5080/api/products -H "Content-Type: application/json" \
  --data-binary @- <<'EOF'
{"sku":"A-101","name":"Магніт Високий Замок","category":"магніт","unit":"шт","quantity":40}
EOF

# прочитати (id з відповіді) -> 200
curl -i http://localhost:5080/api/products/<id>

# замінити -> 200
curl -i -X PUT http://localhost:5080/api/products/<id> -H "Content-Type: application/json" \
  --data-binary @- <<'EOF'
{"sku":"A-101","name":"Магніт XL","category":"магніт","unit":"шт","quantity":55}
EOF

# помилка валідації -> 400
curl -i -X POST http://localhost:5080/api/products -H "Content-Type: application/json" \
  --data-binary @- <<'EOF'
{"sku":"","name":"X","category":"?","unit":"","quantity":-1}
EOF

# видалити -> 204, потім GET -> 404
curl -i -X DELETE http://localhost:5080/api/products/<id>
curl -i http://localhost:5080/api/products/<id>

# пагінація, пошук латиницею і low-stock
curl -i "http://localhost:5080/api/products?page=1&pageSize=5&search=SKU-00"
curl -i "http://localhost:5080/api/products/low-stock?threshold=30"
```

Повторний POST з артикулом, який уже існує, дає 409.

### Реалізація

- DTO окремо від доменної сутності: `Contracts/ProductDto.cs`, `Contracts/SaveProductRequest.cs`.
- Валідація: FluentValidation + `ValidationFilter<T>` через `AddEndpointFilter` (400 з `ValidationProblem`).
- `GlobalExceptionHandler` (`AddExceptionHandler` + `UseExceptionHandler`): `DomainException` → 409, некоректне тіло → 400, решта → 500 без деталей.
- Сховище реєструється як Singleton (`ICatalogStore`) і береться параметром делегата. `InMemoryCatalogStore` зроблено потокобезпечним (`ConcurrentDictionary`).
- Файл `src/Api/Api.http` містить цикл create → read → update → delete → GET (404).

### Тести

```bash
dotnet test
```
Запускає `Core.Tests` і `Api.Tests` (`WebApplicationFactory<Program>`: POST 201 + GET за Location, 400, 409, DELETE → 404, зламаний JSON → 400).

### Команди

`config`, `export`, `import [--file] [--path=...]`, `add --sku= --name= --category= [--unit=] --qty=`.