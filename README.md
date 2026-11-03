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

Команди: `report-top`, `report-low`, `report-groups`, `report-sum`, `report-top-groups`,
`report-groups-query`, `report-moves`, `report-moves-sum`, `report-lazy`, `report-bench`.

### Коли виконується запит
`source.Where(p => p.Quantity < 10)` лише описує запит і нічого не обчислює. Виконання
відбувається під час кожного `query.Count()`, коли запит заново проходить `source`.
Перший виклик побачив 3 дефіцитні позиції, потім у `source` додали «Скобу» з залишком 3,
і другий `Count()` дав 4. `ToList()` виконав запит один раз і зберіг результат, тому
`snapshot.Count` (властивість, а не метод) лишився 4 навіть після `source.Clear()`.

### Синтаксис запитів vs методи
(вставте порівняння: query syntax читається краще для group ... into, але не приймає
StringComparer, тому в ByUnitQuerySyntax довелося писати ToLowerInvariant();
Where/ThenBy/Take зручніші ланцюжком.)

### Власне розширення TopBy
Використано у TopByQuantity і TopUnits; другий ключ (tieBreaker) обов'язковий параметр.

### Stopwatch, 100 000 записів
(вставте свої числа і висновок: повторне перебирання lazy-запиту ≈ у 3 рази повільніше
за ToList один раз; ToList усередині ланцюжка перед Take додає копіювання і примушує
сортувати все, тоді як OrderBy(...).Take(n) може обійтися частковим сортуванням.)

## Lab 07: запуск LINQ-звітів

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