/**
 * Создаёт минимальные валидные PNG там, где файлов ещё нет (для локальной проверки и CI).
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

let created = 0;
for (const rel of REQUIRED_RELATIVE) {
  const abs = join(ROOT, rel);
  if (existsSync(abs)) continue;
  mkdirSync(dirname(abs), { recursive: true });
  writeFileSync(abs, MIN_PNG);
  created += 1;
  console.log(`created placeholder: ${rel}`);
}

if (created === 0) {
  console.log('OK: все PNG уже присутствуют.');
} else {
  console.log(`Готово: создано заглушек: ${created}.`);
}
