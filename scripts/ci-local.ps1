# Локально повторяет основную цепочку CI (без GitHub: нет actionlint/hadolint, lychee, TRX->Checks, Trivy, dependency-review, CodeQL).
# Запуск из корня: pwsh -File scripts/ci-local.ps1
# См. scripts/README.md: -MatchCiNuget -VerifyDocsAssets -RunSmoke
param(
    [switch]$VerifyDocsAssets,
    [switch]$RunSmoke,
    [switch]$MatchCiNuget
)

$ErrorActionPreference = 'Stop'
Set-Location (Resolve-Path (Join-Path $PSScriptRoot '..'))

if ($MatchCiNuget) {
    $env:GITHUB_ACTIONS = 'true'
}

Write-Host '== .NET =='
dotnet restore OrderTracking.sln
dotnet format OrderTracking.sln --verify-no-changes --verbosity minimal
dotnet build OrderTracking.sln -c Release --no-restore
dotnet test OrderTracking.sln -c Release --no-build --verbosity normal

Write-Host '== Frontend =='
Push-Location frontend
try {
    npm ci
    npm run ci
} finally {
    Pop-Location
}

Write-Host '== Docker (нужен Docker Engine) =='
docker compose -f docker-compose.yml config -q
docker compose -f docker-compose.smoke.yml config -q
docker compose build --parallel --pull api worker frontend

if ($VerifyDocsAssets) {
    Write-Host '== PNG для документации (node scripts/verify-docs-assets.mjs) =='
    node (Join-Path $PSScriptRoot 'verify-docs-assets.mjs')
}

if ($RunSmoke) {
    Write-Host '== Smoke: docker-compose.smoke.yml -> GET /health =='
    docker compose -f docker-compose.smoke.yml up -d --wait
    if (-not $?) {
        docker compose -f docker-compose.smoke.yml up -d
    }
    $ok = $false
    for ($i = 0; $i -lt 90; $i++) {
        & curl.exe -fsS --connect-timeout 2 --max-time 12 http://127.0.0.1:15086/health 2>$null
        if ($LASTEXITCODE -eq 0) {
            $ok = $true
            break
        }
        Start-Sleep -Seconds 2
    }
    docker compose -f docker-compose.smoke.yml down -v --remove-orphans 2>$null
    if (-not $ok) {
        Write-Host 'Smoke: /health не ответил за отведённое время' -ForegroundColor Red
        exit 1
    }
}

Write-Host 'OK: локальная проверка завершена.'
