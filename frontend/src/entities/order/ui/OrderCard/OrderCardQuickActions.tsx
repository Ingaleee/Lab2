import { useState } from "react";
import type { Order, OrderStatus } from "../../model/types";
import { useUpdateOrderStatusMutation } from "../../model/mutations";
import s from "./OrderCardQuickActions.module.css";

const statusTransitions: Record<OrderStatus, OrderStatus[]> = {
  New: ["InProgress", "Cancelled"],
  InProgress: ["Delivered", "Cancelled"],
  Delivered: [],
  Cancelled: [],
};

export function OrderCardQuickActions({ order }: { order: Order }) {
  const [isHovered, setIsHovered] = useState(false);
  const mutation = useUpdateOrderStatusMutation(order.id);

  const availableStatuses = statusTransitions[order.status] || [];

  if (availableStatuses.length === 0) return null;

  return (
    <div
      className={s.container}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      {isHovered && (
        <div className={s.actions}>
          {availableStatuses.map((status) => (
            <button
              key={status}
              className={`${s.actionButton} ${s[status.toLowerCase()]}`}
              onClick={(e) => {
                e.stopPropagation();
                mutation.mutate({ status });
              }}
              disabled={mutation.isPending}
              title={`Set status to ${status}`}
            >
              {status}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
