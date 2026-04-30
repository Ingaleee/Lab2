# Документация и иллюстрации

Краткий указатель разделов — в корневом [**README.md**](../README.md#документация). Ниже — детали и ссылки на картинки.

Файлы ниже также ссылаются из корневого README.

## OpenAPI и Jaeger

- [openapi-docs.png](openapi-docs.png) — Swagger UI по `openapi.yaml`
- [jaeger-ui.png](jaeger-ui.png) — Jaeger, сервис `order-tracking-api`

## Скриншоты UI

Полное руководство (Playwright, переменные, типичные ошибки): **[screenshots/README.md](screenshots/README.md)**.

Каталог [screenshots/](screenshots/) — фронтенд, Prometheus, Loki / VictoriaLogs / OpenSearch, Jaeger. Скрипт: [`tools/doc-screenshots/capture.mjs`](../tools/doc-screenshots/capture.mjs), Windows: [`tools/doc-screenshots/capture.cmd`](../tools/doc-screenshots/capture.cmd). Первый ручной запуск в **`tools/doc-screenshots`**: **`npm run setup`**, затем **`npm run capture`**; строгий режим: **`npm run capture:strict`**. Подробности — [screenshots/README.md](screenshots/README.md).

## Логи

[logs-query-languages.md](logs-query-languages.md) — OTLP → collector → Loki, OpenSearch, VictoriaLogs; LogQL, LogsQL, Lucene, DQL. Иллюстрации: `screenshots/logs-grafana-loki-broadcasted.png`, `logs-grafana-victorialogs-outbox.png`, `logs-victorialogs-vmui-worker.png`, также `loki-explore.png`, `victorialogs-query.png`, `opensearch-discover.png`. Краткий блок с теми же скринами — в корневом [README.md](../README.md#наблюдаемость).

## Трейсы

[traces-jaeger.md](traces-jaeger.md) — OTLP, Jaeger UI, теги поиска, связь с логами. Иллюстрации: `screenshots/traces-jaeger-search-api-scatter.png`, `traces-jaeger-search-api-list.png`, `traces-jaeger-search-worker.png`, также `jaeger-search.png`, `jaeger-trace-detail.png`. Краткий блок — в корневом [README.md](../README.md#наблюдаемость).

## Grafana

[grafana-dashboard.md](grafana-dashboard.md) и каталог [grafana/](grafana/) — дашборд и скриншоты (в т.ч. `06-three-log-columns.png` — три колонки логов).

## CI/CD

[ci-cd.md](ci-cd.md) — GitHub Actions (**CI**, **CodeQL**), Dependabot, что проверяется на сервере и что остаётся сделать вручную перед сдачей (**раздел «Перед сдачей»**).

[**scripts/README.md**](../scripts/README.md) — локальные **`ci-local`** с опциями **`MATCH_CI_NUGET`**, **`VERIFY_DOCS_ASSETS`**, **`RUN_SMOKE`**, чтобы повторить часть проверок без push.

Скриншоты вкладки Actions в отчёте не обязательны; для демонстрации инструментов полезны скрины продуктовых UI (Grafana, Jaeger, логи).
