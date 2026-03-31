# Логи приложения: Loki (LogQL), VictoriaLogs (LogsQL), OpenSearch (Lucene / DQL)

Здесь зафиксировано, как логи выходят из .NET, куда их отправляет коллектор и какими языками их можно запрашивать в Loki, VictoriaLogs и OpenSearch.

---

## 1. Откуда берутся логи

- В **ASP.NET Core** я использую стандартный **`ILogger<T>`** и общие настройки из `Logging`.
- В `Program.cs` API и Worker включён **`builder.Logging.AddOpenTelemetry(...)`** с **`AddOtlpExporter`** на endpoint из секции конфигурации `OpenTelemetry:Otlp:Endpoint`. Логи идут по **OTLP** вместе с метриками и трейсами.
- В Docker Compose endpoint указывает на **`otel-collector:4317`**, протокол gRPC.

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

| Экспортёр | Назначение |
|-----------|------------|
| `loki` | Grafana Loki, **LogQL** |
| `elasticsearch` | OpenSearch, индекс `otel-logs`, ECS mapping; **Lucene** и **DQL** в Dashboards |
| `otlphttp/victorialogs` | VictoriaLogs, endpoint OTLP `/insert/opentelemetry/v1/logs`, **LogsQL** |

Три бэкенда получают одни и те же записи логов; имена полей после записи могут немного различаться.

---

## 3. Grafana Loki и язык **LogQL**

- **UI:** Grafana, Explore или панель **Logs**, datasource **Loki**; с хоста `http://localhost:13100`, из сети compose — `loki:3100`.
- Для OTLP-логов в Loki типичен лейбл **`job`**, например `order-tracking/order-tracking-api`; опираться на **`service_name`** в том же виде не всегда корректно. При пустом результате сначала смотрю список лейблов в Explore.

### Примеры LogQL для логов вроде «Broadcasted»

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

**Подсчёт строк за интервал на графике в Grafana:**

```logql
sum(count_over_time({job=~"order-tracking.*"} |= "Broadcasted" [$__interval]))
```

Справка Grafana: [Log queries](https://grafana.com/docs/loki/latest/query/log_queries/).

---

## 4. VictoriaLogs и язык **LogsQL**

- **Хранилище:** сервис `victorialogs` в compose, порт **9428**.
- **Модель:** поля **`_msg`**, **`_stream`**, **`_time`** и потоковые поля из resource attributes OTLP; имя сервиса обычно в **`service.name`**.
- **Grafana:** плагин **`victoriametrics-logs-datasource`**, переменная `GF_INSTALL_PLUGINS` в compose; datasource **VictoriaLogs**, uid `vl`. Запросы **LogsQL** те же, что в панели дашборда.

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

**HTTP API VictoriaLogs:**

- Эндпоинт: `POST /select/logsql/query`
- Параметры: `query`, `start`, `end`, `limit`. Описание: [Querying](https://docs.victoriametrics.com/victorialogs/querying/).

Спецификация языка: [LogsQL](https://docs.victoriametrics.com/victorialogs/logsql/).

---

## 5. OpenSearch: **Lucene** и **DQL**

В OpenSearch Dashboards 2.x для фильтрации в Discover часто используют **DQL**. Параллельно поддерживается синтаксис **Lucene**.

- **UI:** OpenSearch Dashboards, `http://localhost:5601`. В provisioning Grafana заданы индекс **`otel-logs*`**, поле времени **`@timestamp`**, сообщение **`message`**, уровень **`log.level`**, см. `deploy/observability/grafana/provisioning/datasources/datasources.yml`.
- Имя сервиса в ECS обычно в **`service.name`**; структуру полей проверяют по образцу документа в Discover.

### Lucene

```
log.level:Information AND message:*Broadcasted* AND service.name:order-tracking*
```

### DQL

```
log.level: Information and message: *Broadcasted* and service.name: order-tracking*
```

### Запрос `_search` в Dev Tools

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

Имена полей после экспорта в OpenSearch в режиме ECS нужно сверить с фактическим документом в индексе.

---

## 6. Ссылки на код и конфиги

| Что | Где |
|-----|-----|
| OTLP логирование API | `src/OrderTracking.Presentation.Api/Program.cs` |
| OTLP логирование Worker | `src/OrderTracking.Presentation.Worker/Program.cs` |
| Fan-out в три бэкенда | `deploy/observability/otel-collector-config.yml`, секция `logs.exporters` |
| Compose: Loki, OpenSearch, VictoriaLogs, Grafana | `docker-compose.yml` |
| Плагин VictoriaLogs для Grafana | `docker-compose.yml`, `GF_INSTALL_PLUGINS` |
| Datasource в Grafana | `deploy/observability/grafana/provisioning/datasources/datasources.yml` |
| Три панели Logs: LogQL, LogsQL, Lucene | `deploy/observability/grafana/dashboards/order-tracking-metrics.json` |

Скриншоты дашборда: **[grafana-dashboard.md](grafana-dashboard.md)**.
