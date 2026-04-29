/**
 * PNG screenshots into docs/ — run while docker compose stack is up.
 *
 *   cd tools/doc-screenshots && npm install && npx playwright install chromium && npm run capture
 *
 * CLI:  node capture.mjs [--headed] [--strict] [--skip-preflight] [--help] [--version]
 * Env:  DOCS_SCREENSHOT_HOST, GRAFANA_USER, GRAFANA_PASSWORD,
 *       DOCS_CAPTURE_HEADED=1 — как --headed
 *       DOCS_CAPTURE_STRICT=1 — exit 1, если не создан ни один PNG (то же, что --strict)
 *
 * Проверка полноты PNG без браузера (из корня репозитория): node scripts/verify-docs-assets.mjs
 */
import { chromium } from 'playwright';
import { dirname, join } from 'path';
import { fileURLToPath } from 'url';
import { mkdirSync, existsSync, readFileSync } from 'fs';

const __dirname = dirname(fileURLToPath(import.meta.url));
const REPO_ROOT = join(__dirname, '..', '..');

const argv = process.argv.slice(2);

if (argv.includes('--version') || argv.includes('-V')) {
  const pkg = JSON.parse(readFileSync(join(__dirname, 'package.json'), 'utf8'));
  console.log(`doc-screenshots ${pkg.version ?? '?'}  ·  node ${process.version}`);
  process.exit(0);
}

if (argv.includes('--help') || argv.includes('-h')) {
  console.log(`
  doc-screenshots — PNG в docs/ через Playwright (Chromium)

  Запуск (из этого каталога):
    npm run setup          один раз: зависимости + Chromium (то же, что npm install && playwright install)
    npm run capture
    npm run capture:headed
    npm run capture:strict
    npm run help

  Флаги:
    --headed           видимое окно браузера (ещё env DOCS_CAPTURE_HEADED=1)
    --strict           выход с ошибкой, если не создан ни один PNG (ещё DOCS_CAPTURE_STRICT=1)
    --skip-preflight   не проверять API/Grafana через fetch перед браузером
    --version / -V     версия пакета и Node

  Переменные:
    DOCS_SCREENSHOT_HOST          по умолчанию http://127.0.0.1 (без порта; тот же хост, что открывает браузер)
    GRAFANA_USER / GRAFANA_PASSWORD   по умолчанию admin / admin
    DOCS_CAPTURE_HEADED=1        то же, что --headed
    DOCS_CAPTURE_STRICT=1        то же, что --strict

  Коды выхода:
    0 — норма (при --strict ошибка только если не создан ни один PNG)
    1 — оба сервиса preflight недоступны; или --strict при 0 PNG; или необработанное исключение

  Репозиторий (куда пишутся PNG): ${REPO_ROOT}
  Документация: ${join(REPO_ROOT, 'docs', 'screenshots', 'README.md')}
`);
  process.exit(0);
}

const KNOWN_FLAGS = new Set(['--help', '-h', '--headed', '--strict', '--skip-preflight', '--version', '-V']);
const unknownFlags = argv.filter((a) => a.startsWith('-') && !KNOWN_FLAGS.has(a));
if (unknownFlags.length) {
  console.warn('  Неизвестные флаги:', unknownFlags.join(', '));
  console.warn('  Справка: node capture.mjs --help\n');
}

const HEADED =
  process.env.DOCS_CAPTURE_HEADED === '1' ||
  process.env.DOCS_CAPTURE_HEADED === 'true' ||
  argv.includes('--headed');
const SKIP_PREFLIGHT = argv.includes('--skip-preflight');
const STRICT =
  process.env.DOCS_CAPTURE_STRICT === '1' ||
  process.env.DOCS_CAPTURE_STRICT === 'true' ||
  argv.includes('--strict');

const HOST = process.env.DOCS_SCREENSHOT_HOST ?? 'http://127.0.0.1';
const GRAFANA_USER = process.env.GRAFANA_USER ?? 'admin';
const GRAFANA_PASSWORD = process.env.GRAFANA_PASSWORD ?? 'admin';

const delay = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

const stats = { shots: 0, skips: 0 };

function baseUrl() {
  return HOST.replace(/\/$/, '');
}

function url(port, path = '') {
  const base = baseUrl();
  return `${base}:${port}${path.startsWith('/') ? path : `/${path}`}`;
}

function ensureNode() {
  const major = parseInt(process.versions.node.split('.')[0], 10);
  if (major < 18) {
    console.error(`Нужен Node.js 18+ (сейчас ${process.version}). См. frontend/.nvmrc\n`);
    process.exit(1);
  }
}

async function shot(page, fileRel, options = {}) {
  const full = join(REPO_ROOT, fileRel);
  const dir = dirname(full);
  if (!existsSync(dir)) mkdirSync(dir, { recursive: true });
  await page.screenshot({
    path: full,
    animations: 'disabled',
    ...options,
  });
  stats.shots += 1;
  console.log(`  OK  ${fileRel}`);
}

/**
 * @param {{ retries?: number }} opts — retries = число **дополнительных** попыток после первой
 */
async function tryGoto(page, target, label, opts = {}) {
  const extraRetries = opts.retries ?? 1;
  const maxAttempt = extraRetries;
  for (let attempt = 0; attempt <= maxAttempt; attempt++) {
    try {
      await page.goto(target, { waitUntil: 'domcontentloaded', timeout: 60000 });
      await delay(1200);
      return true;
    } catch (e) {
      if (attempt < maxAttempt) {
        await delay(800);
        continue;
      }
      console.warn(`  SKIP ${label}: ${e.message}`);
      stats.skips += 1;
      return false;
    }
  }
  return false;
}

/** Grafana часто крутит не window, а внутренний скролл — двигаем оба */
async function scrollDashboardViewport(page, scrollY) {
  await page.evaluate((y) => {
    window.scrollTo(0, y);
    const selectors = ['.scrollbar-view', '[class*="dashboard-scroll"]', 'main[role="main"]', '.main-view'];
    for (const sel of selectors) {
      document.querySelectorAll(sel).forEach((el) => {
        const max = el.scrollHeight - el.clientHeight;
        if (max > 0) el.scrollTop = Math.min(y, max);
      });
    }
  }, scrollY);
}

async function getDashboardScrollMetrics(page) {
  return page.evaluate(() => {
    const vh = window.innerHeight;
    let scrollHeight = document.documentElement.scrollHeight;
    document.querySelectorAll('.scrollbar-view, [class*="scrollbar-view"]').forEach((el) => {
      scrollHeight = Math.max(scrollHeight, el.scrollHeight + el.getBoundingClientRect().top);
    });
    scrollHeight = Math.max(scrollHeight, document.body?.scrollHeight ?? 0);
    return { scrollHeight, vh };
  });
}

async function trySwaggerShot(page) {
  const paths = ['/swagger/index.html', '/swagger'];
  for (const p of paths) {
    if (await tryGoto(page, url(5086, p), `Swagger (${p})`, { retries: 0 })) {
      await page
        .locator('.swagger-ui, #swagger-ui, .swagger-container')
        .first()
        .waitFor({ state: 'visible', timeout: 12000 })
        .catch(() => {});
      await delay(400);
      await shot(page, 'docs/openapi-docs.png');
      return;
    }
  }
}

async function preflight() {
  const checks = [
    [5086, '/health', 'API'],
    [13001, '/login', 'Grafana'],
  ];
  const failures = [];
  for (const [port, path, name] of checks) {
    try {
      const res = await fetch(url(port, path), { signal: AbortSignal.timeout(8000) });
      if (!res.ok) failures.push(`${name} (${port}): HTTP ${res.status}`);
    } catch {
      failures.push(`${name} (${port}): недоступен`);
    }
  }
  if (failures.length === checks.length) {
    console.error('\n  Ни один ключевой сервис не ответил. Подними стек:\n');
    console.error('    docker compose up -d\n');
    console.error(`  База для скрипта: ${baseUrl()}`);
    console.error('  Или обойти проверку: node capture.mjs --skip-preflight\n');
    process.exit(1);
  }
  if (failures.length) {
    console.warn('\n  Предупреждение preflight:');
    failures.forEach((f) => console.warn(`    — ${f}`));
    console.warn('  Продолжаю — часть кадров может быть пропущена.\n');
  }
}

async function grafanaLogin(page) {
  const loginUrl = url(13001, '/login');
  if (!(await tryGoto(page, loginUrl, 'Grafana login'))) return false;
  const userInput = page.locator('input[name="user"], input#login-input').first();
  const passInput = page.locator('input[name="password"], input#password-input').first();
  if ((await userInput.count()) === 0) {
    console.warn('  Grafana: форма логина не найдена (уже вошли?) — продолжаю');
    return true;
  }
  await userInput.fill(GRAFANA_USER);
  await passInput.fill(GRAFANA_PASSWORD);
  await page.locator('button[type="submit"], button:has-text("Log in"), button:has-text("Войти")').first().click();
  await delay(3000);
  await page.waitForLoadState('networkidle', { timeout: 20000 }).catch(() => {});
  return true;
}

async function main() {
  const t0 = Date.now();
  ensureNode();

  console.log(`\n  doc-screenshots  ·  ${HEADED ? 'headed' : 'headless'}  ·  ${baseUrl()}`);
  if (SKIP_PREFLIGHT) console.log('  preflight: отключён (--skip-preflight)');
  if (STRICT) console.log('  strict: включён (--strict или DOCS_CAPTURE_STRICT=1)');
  console.log(`  repo ${REPO_ROOT}\n`);

  if (!SKIP_PREFLIGHT) await preflight();

  const browser = await chromium.launch({
    headless: !HEADED,
    slowMo: HEADED ? 80 : 0,
  });
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    deviceScaleFactor: 2,
    ignoreHTTPSErrors: true,
  });
  const page = await context.newPage();

  await trySwaggerShot(page);

  const jaegerSearch = url(16686, '/search?service=order-tracking-api&limit=20');
  if (await tryGoto(page, jaegerSearch, 'Jaeger')) {
    await delay(2000);
    await shot(page, 'docs/screenshots/jaeger-search.png');
    await shot(page, 'docs/jaeger-ui.png');
    const tracePickers = [
      page.locator('a[href*="/trace/"]').first(),
      page.locator('[data-testid="trace-link"]').first(),
      page.locator('tbody tr').first(),
    ];
    for (const loc of tracePickers) {
      try {
        if ((await loc.count()) > 0) {
          await loc.click({ timeout: 4000 });
          break;
        }
      } catch {
        /* next */
      }
    }
    await delay(2500);
    await shot(page, 'docs/screenshots/jaeger-trace-detail.png');
  }

  if (await tryGoto(page, url(9090, '/graph'), 'Prometheus', { retries: 1 })) {
    await delay(500);
    await shot(page, 'docs/screenshots/prometheus-graph.png');
  }

  if (await tryGoto(page, url(5173, '/'), 'Frontend')) {
    await delay(800);
    await shot(page, 'docs/screenshots/frontend-app.png');
  }

  if (await tryGoto(page, url(5601, '/app/discover'), 'OpenSearch Discover')) {
    await delay(3500);
    await shot(page, 'docs/screenshots/opensearch-discover.png');
  }

  if (await tryGoto(page, url(9428, '/'), 'VictoriaLogs')) {
    await delay(2000);
    await shot(page, 'docs/screenshots/victorialogs-query.png');
  }

  if (await grafanaLogin(page)) {
    if (await tryGoto(page, url(13001, '/explore?orgId=1'), 'Grafana Explore')) {
      await delay(4000);
      await shot(page, 'docs/screenshots/loki-explore.png');
    }

    const dash = url(
      13001,
      '/d/order-tracking-metrics?orgId=1&from=now-15m&to=now&timezone=browser&refresh=10s'
    );
    if (await tryGoto(page, dash, 'Grafana dashboard', { retries: 1 })) {
      await delay(4500);
      try {
        await page
          .locator('.React-grid-layout, [class*="scrollbar"]')
          .first()
          .waitFor({ state: 'visible', timeout: 15000 });
      } catch {
        /* */
      }
      const { scrollHeight, vh } = await getDashboardScrollMetrics(page);
      const names = [
        '01-dashboard-overview.png',
        '02-transitions-intensity.png',
        '03-status-rate-pipeline.png',
        '04-promql-cheatsheet.png',
        '05-loki-broadcasted.png',
        '06-three-log-columns.png',
      ];
      const steps = names.length;
      const maxY = Math.max(0, scrollHeight - vh);
      for (let i = 0; i < steps; i++) {
        const y = steps <= 1 ? 0 : Math.round((i / (steps - 1)) * maxY);
        await scrollDashboardViewport(page, y);
        await delay(700);
        await shot(page, `docs/grafana/${names[i]}`, { fullPage: false });
      }
    }
  }

  await browser.close();

  const sec = ((Date.now() - t0) / 1000).toFixed(1);
  console.log(`\n  Итого: снято файлов — ${stats.shots}, пропусков шагов — ${stats.skips}, время — ${sec}s`);
  console.log('  Проверь PNG под docs/, затем git status; добавить в индекс: git add docs/  (в Git Bash можно docs/**/*.png)\n');

  if (STRICT && stats.shots === 0) {
    console.error('  strict: ни одного PNG — exit 1');
    console.error('  Проверь docker compose, порты и health URL; для диагностики запусти без --strict или с --skip-preflight.\n');
    process.exit(1);
  }
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
