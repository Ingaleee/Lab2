# Логи приложения: Loki (LogQL), VictoriaLogs (LogsQL), OpenSearch (Lucene / DQL)

Здесь я фиксирую **как именно** у меня уходят логи из .NET (API и Worker), **куда** они записываются и **какими языками запросов** я их достаю — так, чтобы на защите не споткнуться про «KQL / LogQL / что за поля».

---

## 1. Откуда берутся логи

- В **ASP.NET Core** я использую стандартный **`ILogger<T>`** и общие настройки из `Logging`.
- В `Program.cs` API и Worker включён **`builder.Logging.AddOpenTelemetry(...)`** с **`AddOtlpExporter`** на endpoint из конфигурации (`OpenTelemetry:Otlp:Endpoint`), т.е. логи — это часть **OTLP**, вместе с метриками и трейсами.
- В Docker Compose endpoint указывает на **`otel-collector:4317`** (gRPC).

Итого: **один способ эмитации в коде**, дальше — только маршрутизация коллектором.

---

## 2. Куда коллектор их отправляет

Файл `deploy/observability/otel-collector-config.yml`, пайплайн `logs`:

```yaml
logs:
  receivers: [otlp]
  processors: [batch]
  exporters: [loki, elasticsearch, otlphttp/victorialogs]
```

| Экспортёр | Назначение | Зачем мне это в отчёте |
|-----------|------------|------------------------|
| `loki` | Grafana Loki | Классический стек Grafana + **LogQL** |
| `elasticsearch` | OpenSearch (`otel-logs`, ECS mapping) | Запросы **Lucene** в Dev Tools / **DQL** в OpenSearch Dashboards |
| `otlphttp/victorialogs` | VictoriaLogs `/insert/opentelemetry/v1/logs` | Высокопроизводительное хранилище + **LogsQL** |

Три бэкенда получают **одни и те же** log records (с небольшими отличиями в именах полей после трансформаций бэкенда).

---

## 3. Grafana Loki и язык **LogQL**

- **UI:** Grafana → Explore или панель **Logs**, datasource **Loki** (`http://localhost:13100` с хоста, внутри compose — `loki:3100`).
- **Важно:** после экспорта в Loki у OTLP-логов я опираюсь на лейбл **`job`** (например значения вида `order-tracking/order-tracking-api`), а не на «классический» `service_name` в чистом виде. Если запрос пустой — сначала смотрю **лейблы** в Explore.

### Примеры LogQL под мой сценарий (broadcast статуса в UI)

**Фильтр по сервисам стека и подстроке в теле строки:**

```logql
{job=~"order-tracking.*"} |= "Broadcasted"
```

**Только по job API:**

```logql
{job=~".*order-tracking-api.*"} |= "Broadcasted"
```

**С regex по содержимому:**

```logql
{job=~"order-tracking.*"} |~ "(?i)(broadcasted|error)"
```

**Лог как «метрика» — сколько строк за шаг графика (для панели в Grafana):**

```logql
sum(count_over_time({job=~"order-tracking.*"} |= "Broadcasted" [$__interval]))
```

Справка Grafana: [Log queries](https://grafana.com/docs/loki/latest/query/log_queries/).

---

## 4. VictoriaLogs и язык **LogsQL**

- **Хранилище:** сервис `victorialogs` в compose, порт **9428**.
- **Модель:** у VictoriaLogs есть **`_msg`**, **`_stream`**, **`_time`** и поля потока из resource attributes OTLP. Для сервиса почти всегда есть поле вроде **`service.name`** (как в семантических конвенциях OpenTelemetry).
- **Grafana:** я ставлю плагин **`victoriametrics-logs-datasource`** (`GF_INSTALL_PLUGINS` в compose) и datasource **VictoriaLogs** (provision, uid `vl`). В Explore можно писать **LogsQL** так же, как в панели.

### Примеры LogsQL

**Поток по имени сервиса + слово в сообщении:**

```logsql
{service.name=~"order-tracking.*"} Broadcasted
```

Если поле называется иначе — сначала вхожу с широким запросом:

```logsql
*
```

и смотрю, какие поля реально приехали в `_stream` / таблице полей.

**Прямой HTTP API** (удобно показать в отчёте «без Grafana»):

- Эндпоинт: `POST /select/logsql/query`
- Параметры: `query`, `start`, `end`, `limit` (см. [Querying](https://docs.victoriametrics.com/victorialogs/querying/)).

Спецификация языка: [LogsQL](https://docs.victoriametrics.com/victorialogs/logsql/).

---

## 5. OpenSearch: **Lucene** и **DQL** (аналог «KQL-стиля» в Dashboards)

В методичках часто говорят «KQL», имея в виду **язык фильтра в Discover** у Kibana / OpenSearch Dashboards. В OpenSearch 2.x это обычно **DQL** (Dashboards Query Language) в соответствующем режиме; параллельно везде понимается **Lucene**.

- **UI:** OpenSearch Dashboards (`http://localhost:5601`), индексный паттерн у меня provisioning’ом Grafana настроен на **`otel-logs*`**, время **`@timestamp`**, тело **`message`**, уровень **`log.level`** — см. `deploy/observability/grafana/provisioning/datasources/datasources.yml`.
- **Имя сервиса** в ECS чаще **`service.name`** (может быть вложенное поле — проверяю одну «сырую» запись в Discover).

### Lucene (Dev Tools / Discover)

```
log.level:Information AND message:*Broadcasted* AND service.name:order-tracking*
```

### DQL (режим DQL в Discover)

Ориентировочно:

```
log.level: Information and message: *Broadcasted* and service.name: order-tracking*
```

### Прямой `_search` (Dev Tools), если просят «сырой JSON»

```http
GET otel-logs*/_search
{
  "size": 20,
  "sort": [{ "@timestamp": "desc" }],
  "query": {
    "bool": {
      "must": [
        { "match_phrase": { "message": "Broadcasted" }},
        { "prefix": { "service.name": "order-tracking" }}
      ]
    }
  }
}
```

Точные имена полей после `elasticsearch` exporter + ECS я **подтверждаю по одному документу** в индексе — препод может спросить, почему у тебя не совпало имя поля с лекцией.

---

## 6. Что обычно спрашивают на защите (я подготовился заранее)

1. **Почему в Loki один набор лейблов, а в VictoriaLogs другой?** — Потому что Loki exporter кладёт свою модель лейблов, а VL трактует resource attributes OTLP как поля потока; это нормально, важно **не копипастить** LogQL в LogsQL без правок.
2. **Это три копии данных — зачем?** — Чтобы показать **разные языки запросов** и сравнить UX Grafana Explore vs OpenSearch Discover vs VM UI; в проде обычно выбирают 1–2 системы.
3. **Где единая корреляция с трейсом?** — В OTLP-логах есть контекст трассировки; в Jaeger я ищу trace id, в логах — ту же строку или поле (если exporter пробросил).
4. **Нет строк в одной из систем** — Проверить, что collector стартовал после бэкенда, индекс `otel-logs` создался, время и timepicker в UI, и что хотя бы один запрос к API дал `Information`-лог.

---

## 7. Ссылки на код и конфиги (всё уже в репозитории)

| Что | Где |
|-----|-----|
| OTLP логирование API | `src/OrderTracking.Presentation.Api/Program.cs` (`AddOpenTelemetry` + `AddOtlpExporter` для логов) |
| OTLP логирование Worker | `src/OrderTracking.Presentation.Worker/Program.cs` (то же самое) |
| Fan-out логов в **три** бэкенда | `deploy/observability/otel-collector-config.yml` → `exporters: [loki, elasticsearch, otlphttp/victorialogs]` |
| Поднять Loki / OpenSearch / VictoriaLogs / Grafana | `docker-compose.yml` |
| Плагин Grafana для VictoriaLogs | `docker-compose.yml` → `GF_INSTALL_PLUGINS=victoriametrics-logs-datasource` |
| Datasource’ы Grafana (**Prometheus, Loki, OpenSearch, VictoriaLogs**) | `deploy/observability/grafana/provisioning/datasources/datasources.yml` |
| **Три панели Logs** на одном дашборде: LogQL / LogsQL / Lucene | `deploy/observability/grafana/dashboards/order-tracking-metrics.json` (прокрутка вниз: Loki, VictoriaLogs, OpenSearch) |

Дополнительно я описал скрины метрик в **[grafana-dashboard.md](grafana-dashboard.md)**.
