# Участие в проекте

Перед PR выполни те же проверки, что и CI:

- Команды из раздела **«Локальная проверка»** в [`docs/ci-cd.md`](docs/ci-cd.md), **или**
- Через скрипт из корня: **`bash scripts/ci-local.sh`** / **`pwsh -File scripts/ci-local.ps1`** — опции (**строгий NuGet как в CI**, PNG, smoke): **[`scripts/README.md`](scripts/README.md)**.

На сервере CI дополнительно: **lychee** по markdown (в `ci-local` нет — ставь [`lychee`](https://github.com/lycheeverse/lychee) локально при необходимости); PNG (**docs-assets**); после сборки образов — **smoke** (**[`docker-compose.smoke.yml`](docker-compose.smoke.yml)**). Локально smoke можно включить флагом **`RUN_SMOKE`** / **`-RunSmoke`** ([`scripts/README.md`](scripts/README.md)).

Шаблон описания pull request подставляется из [`.github/pull_request_template.md`](.github/pull_request_template.md).

Полный чеклист перед сдачей: раздел **«Перед сдачей»** в [`docs/ci-cd.md`](docs/ci-cd.md).

После съёмки скринов ([`docs/screenshots/README.md`](docs/screenshots/README.md)) можно проверить PNG: **`node scripts/verify-docs-assets.mjs`** или **`VERIFY_DOCS_ASSETS=1`** / **`-VerifyDocsAssets`** (см. [`scripts/README.md`](scripts/README.md)).
