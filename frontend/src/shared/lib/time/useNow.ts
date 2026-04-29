import { useEffect, useState } from "react";

/** Текущее время для сравнений в UI (не использовать Date.now() прямо в render — см. react-hooks/purity). */
export function useNow(intervalMs = 1000): number {
  const [now, setNow] = useState(() => Date.now());
  useEffect(() => {
    const id = window.setInterval(() => setNow(Date.now()), intervalMs);
    return () => window.clearInterval(id);
  }, [intervalMs]);
  return now;
}
