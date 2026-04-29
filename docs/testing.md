# Тестирование

В решении два уровня автотестов на **.NET 9** и отдельный контур для фронтенда.

---

## Проекты

| Проект | Тип | Что покрывает |
|--------|-----|----------------|
| `tests/OrderTracking.Domain.UnitTests` | Unit (xUnit) | Доменная модель и правила |
| `tests/OrderTracking.Presentation.Api.IntegrationTests` | Integration (`WebApplicationFactory`) | Запуск API с **InMemory EF**, без Postgres/Kafka; smoke: `/health`, OpenAPI, заголовки безопасности (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy`) |

Интеграционные тесты подменяют `AppDbContext` на **InMemory**, отключают миграции (`Database:ApplyMigrationsOnStartup=false`), отключают OTLP-логирование и снимают фоновые сервисы **Kafka** и **OrderBookMetricsReporter**, чтобы не требовалась инфраструктура.

Вариант «ближе к продакшену» в CI — отдельный job или секция **`services:`** с контейнером PostgreSQL и реальным провайдером EF; это медленнее и сложнее в поддержке. Для учебного контура выбран InMemory; компромисс описан в [docs/ci-cd.md](ci-cd.md).

---

## Команды

Из корня репозитория:

```bash
# всё решение
dotnet test OrderTracking.sln -c Release

# только интеграционные smoke-тесты API
dotnet test OrderTracking.sln -c Release --filter "FullyQualifiedName~OrderTracking.Presentation.Api.IntegrationTests"

# только доменные unit-тесты
dotnet test OrderTracking.sln -c Release --filter "FullyQualifiedName~OrderTracking.Domain.UnitTests"
```

Версия SDK задаётся в **`global.json`** в корне (`rollForward`: **`latestFeature`** — используется последняя установленная **9.0.*** в пределах полосы функций). В CI используется через **`setup-dotnet`** с `global-json-file`.

В **GitHub Actions** после **`dotnet test`** сохраняются отчёты **TRX** и **покрытие кода** (coverlet, Cobertura) в артефакте **`dotnet-test-results`**; те же TRX при необходимости публикуются в интерфейсе **Checks** (удобно смотреть список тестов на PR). Подробнее — [docs/ci-cd.md](ci-cd.md).

---

## Frontend (локально)

Из каталога **`frontend/`** (те же проверки, что в CI до сборки Docker):

```bash
npm ci
npm run ci
```

Эквивалент **`npm run ci`** (после **`npm ci`**): **`npm audit --audit-level=high`** → **`lint`** → **`format:check`** → **`build`** — как в job **frontend** в CI (`audit` для порога **moderate** остаётся отдельной командой для быстрой проверки).

Длинная форма:

```bash
npm run audit
npm run lint
npm run format:check
npm run build
```

Повторные **`npm run lint`** ускоряются за счёт **`.eslintcache`** в `frontend/` (файл в `.gitignore`; в CI тот же кэш подтягивается через **`actions/cache`**).

Подробнее про npm audit, линтер и пайплайн: [docs/ci-cd.md](ci-cd.md).

---

## Связь с CI

Сборка и прогон тестов в пайплайне описаны в [docs/ci-cd.md](ci-cd.md). Job **docker** (сборка образов и Trivy) выполняется **после** успешного **dotnet** и **frontend** — отдельно от шага `dotnet test`, где интеграционные тесты не поднимают полный Compose.
