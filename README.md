# Order Tracking

Я делаю систему отслеживания заказов с обновлениями в реальном времени через WebSocket (SignalR).

## Как я описываю API (design-first)

Я завёл контракт в [openapi.yaml](src/OrderTracking.Presentation.Api/openapi.yaml), из него NSwag генерирует [OrdersControllerBase.g.cs](src/OrderTracking.Presentation.Api/Generated/OrdersControllerBase.g.cs), а я наследуюсь от него в [OrdersController](src/OrderTracking.Presentation.Api/Controllers/OrdersController.cs) и дописываю бизнес-логику.

![Мой скрин Swagger UI по OpenAPI](docs/openapi-docs.png)

![Мой скрин Jaeger: трейсы order-tracking-api](docs/jaeger-ui.png)

## Что я использую

- .NET 9.0, ASP.NET Core, EF Core
- PostgreSQL
- Kafka (у меня в compose — Redpanda)
- SignalR

## Как я разложил архитектуру

Я выстроил решение в духе Clean Architecture:
- **Domain** — доменная модель, без внешних зависимостей
- **Contracts** — DTO и интеграционные события
- **Application** — абстракции под бизнес-логику
- **Infrastructure** — EF Core, Kafka, у меня ещё outbox
- **Presentation.Api** — REST и SignalR Hub
- **Presentation.Worker** — фоновый сервис, который публикует события из outbox

## Как я запускаю проект

### Через Docker Compose (так удобнее всего)

```bash
docker compose up -d
```

После `up` у меня открываются, в частности:
- API (health): http://localhost:5086/health
- OpenAPI (сырой YAML): http://localhost:5086/api-docs/openapi.yaml
- Swagger UI (если окружение `Development`): http://localhost:5086/swagger
- Frontend из compose: http://localhost:5173
- Jaeger UI: http://localhost:16686
- Grafana: http://localhost:13001 (я захожу как `admin` / `admin`)
- Prometheus: http://localhost:9090
- Loki с хоста: http://localhost:13100
- OpenSearch API: http://localhost:9200
- OpenSearch Dashboards: http://localhost:5601
- VictoriaLogs: http://localhost:9428
- OTLP (gRPC к collector): localhost:4317
- PostgreSQL с моего ПК: **localhost:15432** (внутри Docker-сети контейнеры ходят на `postgres:5432`)
- Kafka (Redpanda) с хоста: **localhost:19092**

### Локально (без полного compose)

Мне всё равно нужны зависимости: проще всего поднять их так:  
`docker compose up -d postgres redpanda jaeger otel-collector`  
или использовать свои инстансы. В [appsettings.json](src/OrderTracking.Presentation.Api/appsettings.json) я оставил настройки под подключение **с хоста** к тому, что в compose: Postgres **localhost:15432**, Kafka **localhost:19092**, OTLP **http://localhost:4317**.

```bash
# API
cd src/OrderTracking.Presentation.Api
dotnet run

# Worker — в другом терминале
cd src/OrderTracking.Presentation.Worker
dotnet run
```

## Как я проверяю, что всё живое

### Нагрузка под метрики и логи в Grafana

В **Docker Compose** на сервисе **api** я по умолчанию включил сценарий **`DemoTraffic__*`** (см. `docker-compose.yml`): через примерно 1–2 минуты после `up` метрики начинают наполняться сами. Чтобы отключить: убираю `DemoTraffic__Enabled=true` или ставлю `DemoTraffic__Enabled=false`.

Дальше можно добить нагрузку вручную командами `curl` из этого раздела.

### 1. Создать заказ

```bash
curl -X POST http://localhost:5086/api/orders \
  -H "Content-Type: application/json" \
  -d '{"orderNumber":"ORD-001","description":"Test order"}'
```

Я сохраняю `id` из ответа.

### 2. Поменять статус

```bash
curl -X PATCH http://localhost:5086/api/orders/{ORDER_ID}/status \
  -H "Content-Type: application/json" \
  -d '{"status":"InProgress"}'
```

### 3. Я смотрю цепочку событий

**Outbox в БД:**
```bash
docker compose exec postgres psql -U postgres -d order_tracking \
  -c "SELECT id, type, status FROM outbox_messages ORDER BY occurred_at DESC LIMIT 5;"
```

**Логи Worker (публикация в Kafka):**
```bash
docker compose logs worker | grep "Kafka published"
```

**Логи API (обработка и broadcast):**
```bash
docker compose logs api | grep "Broadcasted status update"
```

**ProcessedEvents (идемпотентность):**
```bash
docker compose exec postgres psql -U postgres -d order_tracking \
  -c "SELECT event_id, processed_at FROM processed_events ORDER BY processed_at DESC LIMIT 5;"
```

### 4. SignalR

Я ставлю зависимости и гоняю тестовый клиент:

```bash
cd tools
npm install
node signalr-client.js
```

Если потом меняю статус через API, в консоли клиента я вижу событие.

## Мои эндпоинты API

- `POST /api/orders` — создать заказ
- `GET /api/orders` — список
- `GET /api/orders/{id}` — карточка
- `PATCH /api/orders/{id}/status` — смена статуса

## Как я использую SignalR

**Hub:** `/hubs/orders`

**Методы (я их вызываю с клиента):**
- `JoinOrdersList()` — подписка на все заказы
- `JoinOrder(orderId)` — подписка на один заказ

**События (я на них подписываюсь):**
- `orderStatusChanged` — статус изменился

## Как я задал переходы статусов

- `New` → `InProgress` или `Cancelled`
- `InProgress` → `Delivered` или `Cancelled`
- `Delivered`, `Cancelled` — финальные, дальше я их не трогаю

## Наблюдаемость (как я это подключил)

Я использую **OpenTelemetry** для распределённой трассировки и метрик.

### Где я смотрю трейсы и дашборды

1. **Трейсы** — Jaeger: http://localhost:16686 — выбираю сервис `order-tracking-api` или `order-tracking-worker`, жму «Find Traces».
2. **Метрики и дашборды** — Grafana: http://localhost:13001 (Prometheus, Loki, OpenSearch datasource и **VictoriaLogs** через плагин `victoriametrics-logs-datasource`, см. `docker-compose.yml`). Отдельный UI VictoriaLogs: http://localhost:9428.
3. **Сырые метрики** — Prometheus: http://localhost:9090 — у меня он скрейпит **`/metrics`** у **api:8080** и **worker:9464**.

Текстовое описание моего дашборда Grafana и скрины для отчёта я вынес в [docs/grafana-dashboard.md](docs/grafana-dashboard.md).

**Логи и языки запросов** — уже в проекте: OTLP из API/Worker → collector → **Loki + OpenSearch + VictoriaLogs**; в Grafana на дашборде **три панели Logs** (LogQL / LogsQL / Lucene). Подробно: [docs/logs-query-languages.md](docs/logs-query-languages.md).

### Что именно я трейсю

- HTTP (ASP.NET Core)
- запросы EF Core к БД
- свои спаны:
  - `Outbox.Dispatch` — worker разбирает outbox
  - `Kafka.Produce` — публикация в Kafka
  - `Kafka.Consume` — потребление в API
  - `SignalR.Broadcast` — раздача события клиентам
