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

