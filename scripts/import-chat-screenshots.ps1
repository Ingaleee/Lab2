<#
.SYNOPSIS
  Копирует PNG из кэша Cursor (куда попадают картинки из чата) в docs/screenshots/
  под именами, которые ждут README и docs/*.md.

.DESCRIPTION
  Ассистент в Cursor не может закоммитить бинарники с твоего диска — только текст.
  После запуска этого скрипта сделай: git add docs/screenshots/*.png && git commit && git push

.PARAMETER AssetsRoot
  Папка с файлами вида ...image-<guid>.png. По умолчанию ищет типичные места Cursor.

.EXAMPLE
  pwsh -File scripts/import-chat-screenshots.ps1
#>
param(
    [string[]] $AssetsRoot = @()
)

$ErrorActionPreference = 'Stop'
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$DestDir = Join-Path $RepoRoot 'docs/screenshots'
New-Item -ItemType Directory -Force -Path $DestDir | Out-Null

if ($AssetsRoot.Count -eq 0) {
    $AssetsRoot = @(
        (Join-Path $env:USERPROFILE '.cursor\projects\c-Users-CLEAR-Desktop-Lab2\assets'),
        (Join-Path $env:USERPROFILE '.cursor\projects\Lab2\assets')
    ) | Where-Object { Test-Path $_ }
}

# workspaceStorage: долгие имена ...images_image-<hash>.png
$ws = Join-Path $env:APPDATA 'Cursor\User\workspaceStorage'
if (Test-Path $ws) {
    $AssetsRoot += $ws
}

$map = [ordered]@{
    # Jaeger (чат)
    '*745c8971*'                          = 'traces-jaeger-search-api-scatter.png'
    '*46ed587c*'                          = 'traces-jaeger-search-api-list.png'
    '*9c4a3e60*'                          = 'traces-jaeger-search-worker.png'
    # Логи (чат)
    '*6d45f273*'                          = 'logs-grafana-loki-broadcasted.png'
    '*513fe03b*'                          = 'logs-grafana-victorialogs-outbox.png'
    '*20003168*'                          = 'logs-victorialogs-vmui-worker.png'
    # Метрики Grafana / Prometheus (чат)
    '*b2d4f9ca*'                          = 'metrics-prometheus-targets.png'
    '*1645bee2*'                          = 'metrics-grafana-explore-runtime-overview.png'
    '*8a63cebd*'                          = 'metrics-grafana-explore-runtime-overview-alt.png'
    '*75d300c8*'                          = 'metrics-grafana-explore-dotnet-deep.png'
    '*1018ffcc*'                          = 'metrics-grafana-explore-http-outbound.png'
    '*032224cb*'                          = 'metrics-grafana-explore-http-server-business.png'
    '*afe958cf*'                          = 'metrics-grafana-explore-scrape-target-health.png'
}

function Find-PngMatch {
    param([string]$GlobPattern)
    foreach ($root in $AssetsRoot) {
        if (-not (Test-Path $root)) { continue }
        $found = Get-ChildItem -Path $root -Filter '*.png' -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -like $GlobPattern } |
            Select-Object -First 1
        if ($found) { return $found }
    }
    return $null
}

Write-Host "Repo: $RepoRoot"
Write-Host "Dest: $DestDir"
Write-Host ""

$copied = 0
$missing = 0

foreach ($entry in $map.GetEnumerator()) {
    $pattern = $entry.Key
    $destName = $entry.Value
    $src = Find-PngMatch -GlobPattern $pattern
    if ($src) {
        $dest = Join-Path $DestDir $destName
        Copy-Item -LiteralPath $src.FullName -Destination $dest -Force
        Write-Host "[OK]   $destName"
        Write-Host "       <= $($src.FullName)"
        $copied++
    }
    else {
        Write-Host "[SKIP] $destName (нет файла по шаблону $pattern)"
        $missing++
    }
}

Write-Host ""
Write-Host "Готово: скопировано $copied, не найдено $missing."
if ($missing -gt 0) {
    Write-Host ""
    Write-Host "Если много SKIP: положи PNG вручную в docs/screenshots/ с именами из таблицы выше,"
    Write-Host "или поправь в скрипте пути `$AssetsRoot / шаблоны имён под свой Cursor."
}
Write-Host ""
Write-Host "Дальше из корня репозитория:"
Write-Host "  git add docs/screenshots/*.png"
Write-Host '  git commit -m "docs: скриншоты наблюдаемости"'
Write-Host "  git push"
