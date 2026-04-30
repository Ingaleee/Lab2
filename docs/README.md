# Документация проекта

Этот каталог содержит дополнительные материалы к основному [README](../README.md): CI/CD, тестирование, наблюдаемость, трассировки и правила обновления скриншотов.

## Навигация

| Документ | Описание |
|---|---|
| [ci-cd.md](ci-cd.md) | устройство GitHub Actions, CodeQL, проверок качества и security-gate |
| [testing.md](testing.md) | unit- и integration-тесты, локальные команды запуска |
| [logs-query-languages.md](logs-query-languages.md) | как читать логи в Loki, VictoriaLogs и OpenSearch |
| [traces-jaeger.md](traces-jaeger.md) | работа с Jaeger, поиск трасс и связь с логами |
| [grafana-dashboard.md](grafana-dashboard.md) | панели Grafana и смысл основных метрик |
| [screenshots/README.md](screenshots/README.md) | как переснять PNG для README и документации |

## Иллюстрации

Скриншоты, которые используются в README, лежат в [screenshots](screenshots). Они показывают:

- состояние Prometheus targets;
- метрики .NET runtime, HTTP client/server, Kestrel и scrape;
- логи в Grafana Loki, VictoriaLogs и VMUI;
- трассировки API и worker в Jaeger.

Отдельные изображения для общей документации лежат прямо в `docs/`:

| Файл | Содержание |
|---|---|
| [openapi-docs.png](openapi-docs.png) | Swagger/OpenAPI UI |
| [jaeger-ui.png](jaeger-ui.png) | общий вид Jaeger UI |

## Обновление скриншотов

1. Поднять стек:

```bash
docker compose up -d
```

2. Запустить съемку:

```bash
cd tools/doc-screenshots
npm run setup
npm run capture
```

На Windows можно использовать wrapper:

```bat
tools\doc-screenshots\capture.cmd
```

3. Проверить наличие ожидаемых файлов:

```bash
node scripts/verify-docs-assets.mjs
```

Имена PNG зафиксированы в markdown. Если картинку нужно заменить вручную, сохраняйте новый файл под тем же именем.
