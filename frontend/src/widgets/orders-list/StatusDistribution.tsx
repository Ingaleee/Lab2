import type { Order, OrderStatus } from "../../entities/order/model/types";
import s from "./StatusDistribution.module.css";

const statusLabels: Record<OrderStatus, string> = {
  New: "New",
  InProgress: "InProgress",
  Delivered: "Delivered",
  Cancelled: "Cancelled",
};

export function StatusDistribution({ orders }: { orders: Order[] }) {
  const counts = orders.reduce(
    (acc, order) => {
      acc[order.status] = (acc[order.status] || 0) + 1;
      return acc;
    },
    {} as Record<OrderStatus, number>
  );

  const items = (Object.keys(statusLabels) as OrderStatus[])
    .filter((status) => counts[status] > 0)
    .map((status) => ({
      status,
      label: statusLabels[status],
      count: counts[status],
    }));

  if (items.length === 0) return null;

  return (
    <div className={s.container}>
      {items.map((item, idx) => (
        <span key={item.status} className={s.item}>
          <span className={s.label}>{item.label}</span>
          <span className={s.count}>{item.count}</span>
          {idx < items.length - 1 && <span className={s.separator}>·</span>}
        </span>
      ))}
    </div>
  );
}
