@echo off
setlocal
cd /d "%~dp0"

if /i "%~1"=="help" (
  echo.
  where node >nul 2>nul
  if errorlevel 1 (
    echo  Node.js не найден в PATH.
    exit /b 1
  )
  node capture.mjs --help
  exit /b 0
)

if /i "%~1"=="version" (
  where node >nul 2>nul
  if errorlevel 1 (
    echo  Node.js не найден в PATH.
    exit /b 1
  )
  node capture.mjs --version
  exit /b 0
)

set "SCRIPT=capture"
if /i "%~1"=="headed" (
  set "SCRIPT=capture:headed"
  goto :strip_first
)
if /i "%~1"=="debug" (
  set "SCRIPT=capture:headed"
  goto :strip_first
)
if /i "%~1"=="strict" (
  set "SCRIPT=capture:strict"
  goto :strip_first
)
goto :run

:strip_first
shift

:run
echo.
echo  === doc-screenshots ===
echo  cwd: %CD%
echo.
echo  Перед запуском: docker compose up -d  (из корня репозитория^)
echo.
echo  Примеры:
echo    capture.cmd help                  — справка ^(без npm install^)
echo    capture.cmd version               — версия скрипта и Node
echo    capture.cmd                       — headless
echo    capture.cmd headed                — видимый Chromium
echo    capture.cmd strict                — ошибка, если 0 PNG ^(CI^)
echo    capture.cmd headed --skip-preflight
echo    capture.cmd --skip-preflight
echo.
echo  Руками без батника ^(macOS/Linux или свой порядок^): npm run setup, затем npm run capture
echo  Полное руководство: ..\..\docs\screenshots\README.md
echo.

call npm install
if errorlevel 1 goto :fail
call npx playwright install chromium
if errorlevel 1 goto :fail

call npm run %SCRIPT% -- %*
if errorlevel 1 goto :fail

echo.
echo  Готово.
echo  Из корня репозитория: git add docs/ ^&^& git status
goto :eof

:fail
echo.
echo  Ошибка. Проверь Node 18+, сеть, docker compose и порты.
echo  Если fetch из Node не видит localhost: npm run capture -- --skip-preflight
echo  Режим strict при 0 PNG: capture.cmd strict
echo  Справка: capture.cmd help          Версия: capture.cmd version
exit /b 1
