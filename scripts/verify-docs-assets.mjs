/**
 * Проверяет наличие PNG под документацию (те же пути, что создаёт tools/doc-screenshots/capture.mjs).
 * Запуск из корня: node scripts/verify-docs-assets.mjs
 * Код выхода: 0 — все файлы есть; 1 — есть пропуски.
 */
import { existsSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { REQUIRED_RELATIVE } from './docs-assets-required.mjs';

const __dirname = dirname(fileURLToPath(import.meta.url));
const ROOT = join(__dirname, '..');

const missing = REQUIRED_RELATIVE.filter((rel) => !existsSync(join(ROOT, rel)));

if (missing.length > 0) {
  console.error('Не хватает PNG для документации:\n');
  missing.forEach((m) => console.error(`  ${m}`));
  console.error('\nПодними стек (docker compose up -d), затем съёмку: docs/screenshots/README.md');
  console.error('Или tools/doc-screenshots: npm run capture\n');
  process.exit(1);
}

console.log(`OK: все ${REQUIRED_RELATIVE.length} ожидаемых PNG для документации на месте.`);
