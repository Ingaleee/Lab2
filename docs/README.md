# Мои материалы для README и отчёта

Здесь я храню скриншоты, на которые ссылаюсь из корневого [README.md](../README.md).

## OpenAPI и Jaeger (для раздела про design-first)

Я положил сюда:

- **openapi-docs.png** — мой скрин Swagger UI (документация API по моему `openapi.yaml`)
- **jaeger-ui.png** — мой скрин Jaeger UI (трейсы `order-tracking-api`)

В основном README они вставлены в блок «Как я описываю API» и «Наблюдаемость».

## Grafana (обязательно для отчёта по метрикам)

Я описал панели дашборда у себя в **[grafana-dashboard.md](grafana-dashboard.md)** и приложил скрины в папке **`grafana/`**:

| Файл | Что я на нём показываю |
|------|-------------------------|
| [grafana/01-dashboard-overview.png](grafana/01-dashboard-overview.png) | Обзор: KPI, запасы по статусам, SLA, счётчики PromQL |
| [grafana/02-transitions-intensity.png](grafana/02-transitions-intensity.png) | Переходы по статусам, таблица, график интенсивности |
| [grafana/03-status-rate-pipeline.png](grafana/03-status-rate-pipeline.png) | Rate переходов и конвейер outbox → Kafka → UI |
| [grafana/04-promql-cheatsheet.png](grafana/04-promql-cheatsheet.png) | Шпаргалка PromQL / LogQL на дашборде |
| [grafana/05-loki-broadcasted.png](grafana/05-loki-broadcasted.png) | Loki: логи доставки статуса (`Broadcasted`) |

Чтобы быстро открыть файл в редакторе, я обычно жму **Ctrl+P** и набираю, например, `01-dashboard-overview`.
