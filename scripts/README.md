# Скрипты локальной проверки

Запуск **из корня репозитория**.

| Скрипт | Назначение |
|--------|------------|
| [`ci-local.sh`](ci-local.sh) | Bash (Linux, macOS, Git Bash на Windows): основная цепочка как в CI без GitHub-only шагов |
| [`ci-local.ps1`](ci-local.ps1) | PowerShell на Windows |
| [`import-chat-screenshots.ps1`](import-chat-screenshots.ps1) | Копирует PNG из кэша Cursor (картинки из чата) в **`docs/screenshots/`** под именами для README → затем **`git add`** и push |

По умолчанию **нет**: actionlint/hadolint, TRX→Checks, Trivy, dependency-review, CodeQL, lychee — они выполняются только в **GitHub Actions**.

## Переменные окружения (`ci-local.sh`)

| Переменная | Значение | Эффект |
|------------|----------|--------|
| `MATCH_CI_NUGET=1` | перед запуском | Выставляет `GITHUB_ACTIONS=true` для шагов **dotnet**, чтобы сработали те же правила **NU1902–NU1904**, что в CI ([`Directory.Build.props`](../Directory.Build.props)). |
| `VERIFY_DOCS_ASSETS=1` | после Docker build | Запускает [`verify-docs-assets.mjs`](verify-docs-assets.mjs) (те же PNG, что ждёт job **docs-assets**). |
| `RUN_SMOKE=1` | после Docker build | Поднимает [`docker-compose.smoke.yml`](../docker-compose.smoke.yml) и проверяет **GET /health** (как в job **Docker** на сервере), затем **`down -v`**. |

Пример полной приближённой проверки перед push:

```bash
MATCH_CI_NUGET=1 VERIFY_DOCS_ASSETS=1 RUN_SMOKE=1 bash scripts/ci-local.sh
```

## Параметры (`ci-local.ps1`)

| Параметр | Эффект |
|----------|--------|
| `-VerifyDocsAssets` | После сборки образов — `node scripts/verify-docs-assets.mjs` |
| `-RunSmoke` | После сборки образов — smoke через `docker-compose.smoke.yml` |
| `-MatchCiNuget` | Перед **dotnet** задаёт `$env:GITHUB_ACTIONS='true'` на время скрипта |

```powershell
pwsh -File scripts/ci-local.ps1 -MatchCiNuget -VerifyDocsAssets -RunSmoke
```
