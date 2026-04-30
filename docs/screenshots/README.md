# Скриншоты для документации

В этой папке лежат PNG, которые подключаются в корневом [README](../../README.md) и документах из каталога [docs](..). Скриншоты нужны не как декоративные картинки, а как доказательство работы observability-стека: Prometheus видит targets, Grafana отображает метрики, Loki/VictoriaLogs принимают логи, Jaeger показывает трассировки.

## Ожидаемые файлы

| Файл | Что показывает |
|---|---|
| `metrics-prometheus-targets.png` | Prometheus `/targets`: API и worker в состоянии `UP` |
| `metrics-grafana-explore-runtime-overview.png` | общий обзор runtime-метрик .NET |
| `metrics-grafana-explore-runtime-overview-alt.png` | дополнительный экран runtime-метрик |
| `metrics-grafana-explore-dotnet-deep.png` | GC, JIT, CPU, память, thread pool |
| `metrics-grafana-explore-http-outbound.png` | исходящие HTTP-запросы и HttpClient |
| `metrics-grafana-explore-http-server-business.png` | входящий HTTP, Kestrel и доменные счетчики |
| `metrics-grafana-explore-business-counters.png` | продуктовые счетчики заказов, Kafka и outbox |
| `metrics-grafana-explore-scrape-target-health.png` | scrape-метрики, `target_info`, `up` |
| `logs-grafana-loki-broadcasted.png` | поиск `Broadcasted` в Loki через Grafana |
| `logs-grafana-victorialogs-outbox.png` | outbox/EF Core логи в VictoriaLogs datasource |
| `logs-victorialogs-vmui-worker.png` | worker-логи в VictoriaLogs VMUI |
| `loki-explore.png` | Grafana Explore для логов |
| `victorialogs-query.png` | запросы к VictoriaLogs |
| `opensearch-discover.png` | просмотр логов в OpenSearch Dashboards |
| `traces-jaeger-search-api-scatter.png` | Jaeger scatter для `order-tracking-api` |
| `traces-jaeger-search-api-list.png` | список трасс API |
| `traces-jaeger-search-worker.png` | трассы `order-tracking-worker` |
| `frontend-app.png` | frontend приложения |
| `prometheus-graph.png` | Prometheus graph |
| `jaeger-search.png` | общий поиск в Jaeger |
| `jaeger-trace-detail.png` | детализация одной трассы |

## Как переснять

Сначала поднимите весь стек из корня репозитория:

```bash
docker compose up -d
```

Затем запустите Playwright-съемку:

```bash
cd tools/doc-screenshots
npm run setup
npm run capture
```

На Windows доступен wrapper:

```bat
tools\doc-screenshots\capture.cmd
```

Для строгой проверки, полезной перед сдачей или CI:

```bash
npm run capture:strict
```

## Проверка

Из корня репозитория:

```bash
node scripts/verify-docs-assets.mjs
```

Команда проверяет, что все PNG, на которые ссылается документация, существуют. Если файл заменяется вручную, оставляйте то же имя, иначе ссылки в Markdown перестанут работать.

## Практические замечания

- Grafana и OpenSearch могут открываться не сразу: после `docker compose up -d` подождите 1-3 минуты.
- Для полезных метрик и трасс нужен трафик. В compose включен `DemoTraffic__Enabled=true`, поэтому данные появляются автоматически после старта API.
- Если скриншот пустой, сначала проверьте соответствующий URL вручную: Prometheus `9090`, Grafana `13001`, Jaeger `16686`, VictoriaLogs `9428`.
- После обновления скриншотов добавьте изменения из `docs/` в git.
