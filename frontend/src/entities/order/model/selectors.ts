import type { Order, OrderStatus } from "./types";

export type OrdersSort = "Newest" | "Oldest";

export type OrdersListFilter = {
  search: string;
  status: OrderStatus | "All";
  sort: OrdersSort;
};

export function filterAndSortOrders(orders: Order[], filter: OrdersListFilter): Order[] {
  const search = filter.search.trim().toLowerCase();

  let result = orders;

  if (filter.status !== "All") {
    result = result.filter((o) => o.status === filter.status);
  }

  if (search.length > 0) {
    result = result.filter((o) => {
      const text = `${o.orderNumber} ${o.description}`.toLowerCase();
      return text.includes(search);
    });
  }

  result = [...result].sort((a, b) => {
    const aTime = new Date(a.createdAt).getTime();
    const bTime = new Date(b.createdAt).getTime();

    return filter.sort === "Newest" ? bTime - aTime : aTime - bTime;
  });

  return result;
}
