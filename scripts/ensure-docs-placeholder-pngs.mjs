/**
 * Создаёт минимальные валидные PNG там, где файлов ещё нет (для локальной проверки и CI).
 * С флагом --force перезаписывает все пути из docs-assets-required.mjs (чтобы на GitHub не было 404).
 * Замените на реальные скриншоты: docs/screenshots/README.md, npm run capture в tools/doc-screenshots.
 */
import { existsSync, mkdirSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { REQUIRED_RELATIVE } from './docs-assets-required.mjs';

const __dirname = dirname(fileURLToPath(import.meta.url));
const ROOT = join(__dirname, '..');

const MIN_PNG = Buffer.from(
  'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==',
  'base64',
);

const FORCE = process.argv.includes('--force');

if (FORCE) {
  console.warn(
    'Внимание: --force перезапишет все PNG из REQUIRED_RELATIVE (включая уже готовые скриншоты).',
  );
}

let created = 0;
let skipped = 0;
for (const rel of REQUIRED_RELATIVE) {
  const abs = join(ROOT, rel);
  const existed = existsSync(abs);
  if (existed && !FORCE) {
    skipped += 1;
    continue;
  }
  mkdirSync(dirname(abs), { recursive: true });
  writeFileSync(abs, MIN_PNG);
  created += 1;
  console.log(`${existed && FORCE ? 'перезаписан' : 'created'}: ${rel}`);
}

if (FORCE) {
  console.log(`Режим --force: записано файлов — ${created}.`);
} else if (created === 0) {
  console.log('OK: все PNG уже присутствуют.');
} else {
  console.log(`Готово: создано заглушек: ${created}.`);
}
