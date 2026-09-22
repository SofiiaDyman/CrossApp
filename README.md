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