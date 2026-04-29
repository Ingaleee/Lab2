#!/usr/bin/env bash
# Локально повторяет основную цепочку CI (без GitHub: нет actionlint/hadolint, lychee, TRX→Checks, Trivy, dependency-review, CodeQL).
# Запуск из корня: bash scripts/ci-local.sh
# Опции: MATCH_CI_NUGET=1 VERIFY_DOCS_ASSETS=1 RUN_SMOKE=1 — см. scripts/README.md
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

if [ "${MATCH_CI_NUGET:-}" = "1" ]; then
  export GITHUB_ACTIONS=true
fi

echo "== .NET =="
dotnet restore OrderTracking.sln
dotnet format OrderTracking.sln --verify-no-changes --verbosity minimal
dotnet build OrderTracking.sln -c Release --no-restore
dotnet test OrderTracking.sln -c Release --no-build --verbosity normal

echo "== Frontend =="
pushd frontend >/dev/null
npm ci
npm run ci
popd >/dev/null

echo "== Docker (нужен запущенный Docker Engine) =="
docker compose -f docker-compose.yml config -q
docker compose -f docker-compose.smoke.yml config -q
docker compose build --parallel --pull api worker frontend

if [ "${VERIFY_DOCS_ASSETS:-}" = "1" ]; then
  echo "== PNG для документации (node scripts/verify-docs-assets.mjs) =="
  node scripts/verify-docs-assets.mjs
fi

if [ "${RUN_SMOKE:-}" = "1" ]; then
  echo "== Smoke: docker-compose.smoke.yml → GET /health =="
  docker compose -f docker-compose.smoke.yml up -d --wait || docker compose -f docker-compose.smoke.yml up -d
  ok=
  for _ in $(seq 1 90); do
    if curl -fsS --connect-timeout 2 --max-time 12 http://127.0.0.1:15086/health; then
      ok=1
      break
    fi
    sleep 2
  done
  docker compose -f docker-compose.smoke.yml down -v --remove-orphans || true
  if [ -z "${ok:-}" ]; then
    echo "Smoke: /health не ответил за отведённое время" >&2
    exit 1
  fi
fi

echo "OK: локальная проверка завершена."
