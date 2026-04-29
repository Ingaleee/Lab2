## Изменения

Кратко опиши, что сделано и зачем.

## Перед мержем

- [ ] Локально выполнены проверки из раздела **«Локальная проверка»** в [`docs/ci-cd.md`](docs/ci-cd.md) и/или [`scripts/README.md`](scripts/README.md) (`ci-local` с нужными флагами при необходимости).
- [ ] В **GitHub Actions** зелёные прогоны workflow **[CI](.github/workflows/ci.yml)** (**Lint — YAML + Dockerfile**, **.NET**, **Frontend**, **Docs — PNG, ссылки**, **Docker**) и **[CodeQL](.github/workflows/codeql.yml)** (анализ C#).
- [ ] На **pull request** при требованиях к зависимостям учтён статус **Dependency review** (отдельная job; при необходимости включи её в branch protection вместе с **«CI — все проверки пройдены»**).
