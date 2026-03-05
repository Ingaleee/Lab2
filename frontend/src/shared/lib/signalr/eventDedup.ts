export class LruSet {
  private readonly limit: number;
  private readonly map = new Map<string, true>();

  constructor(limit = 500) {
    this.limit = limit;
  }

  has(id: string) {
    return this.map.has(id);
  }

  add(id: string) {
    if (this.map.has(id)) return;

    this.map.set(id, true);
    if (this.map.size > this.limit) {
      const firstKey = this.map.keys().next().value as string | undefined;
      if (firstKey) this.map.delete(firstKey);
    }
  }
}
