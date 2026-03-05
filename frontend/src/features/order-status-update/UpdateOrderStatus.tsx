import { useMemo, useState } from "react";
import type { Order, OrderStatus } from "../../entities/order/model/types";
import { useUpdateOrderStatusMutation } from "../../entities/order/model/mutations";
import { Select } from "../../shared/ui/Select/Select";
import { Button } from "../../shared/ui/Button/Button";
import s from "./UpdateOrderStatus.module.css";

const statuses: OrderStatus[] = ["New", "InProgress", "Delivered", "Cancelled"];

export function UpdateOrderStatus({ order }: { order: Order }) {
  const mutation = useUpdateOrderStatusMutation(order.id);

  const allowedStatuses = useMemo(() => statuses, []);
  const [nextStatus, setNextStatus] = useState<OrderStatus>(order.status);

  return (
    <div className={s.form}>
      <div className={s.field}>
        <label className={s.label}>Update Status</label>
        <div className={s.controls}>
          <Select
            value={nextStatus}
            onChange={(e) => setNextStatus(e.target.value as OrderStatus)}
            className={s.select}
          >
            {allowedStatuses.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </Select>
          <Button
            disabled={mutation.isPending || nextStatus === order.status}
            onClick={() => mutation.mutate({ status: nextStatus })}
            className={s.submitButton}
          >
            {mutation.isPending ? "Updating..." : "Update"}
          </Button>
        </div>
      </div>
    </div>
  );
}
