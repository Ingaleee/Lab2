import type { OrderStatus } from "../../model/types";
import s from "./OrderStatusBadge.module.css";

const statusClassMap: Record<OrderStatus, string> = {
  New: s.new,
  InProgress: s.inProgress,
  Delivered: s.delivered,
  Cancelled: s.cancelled,
};

export function OrderStatusBadge({ status }: { status: OrderStatus }) {
  return (
    <span className={`${s.badge} ${statusClassMap[status]}`}>
      <span className={s.dot}></span>
      {status}
    </span>
  );
}
