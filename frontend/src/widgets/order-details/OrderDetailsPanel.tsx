import { X, Circle } from "lucide-react";
import { useOrderByIdQuery } from "../../entities/order/model/queries";
import { UpdateOrderStatus } from "../../features/order-status-update/UpdateOrderStatus";
import { OrderStatusBadge } from "../../entities/order/ui/OrderStatusBadge/OrderStatusBadge";
import { StatusStepper } from "../../shared/ui/StatusStepper/StatusStepper";
import { ActivityFeed } from "../../shared/ui/ActivityFeed/ActivityFeed";
import { useActivityFeed } from "../../shared/lib/activity/useActivityFeed";
import { Card } from "../../shared/ui/Card/Card";
import s from "./OrderDetailsPanel.module.css";

export function OrderDetailsPanel({
  orderId,
  onClose,
}: {
  orderId: string;
  onClose: () => void;
}) {
  const q = useOrderByIdQuery(orderId);
  const activities = useActivityFeed(orderId, q.data);

  if (q.isLoading) {
    return (
      <div className={s.panel}>
        <div className={s.loading}>Loading order...</div>
      </div>
    );
  }

  if (q.isError || !q.data) {
    return (
      <div className={s.panel}>
        <div className={s.error}>Failed to load order</div>
      </div>
    );
  }

  const order = q.data;
  const createdDate = new Date(order.createdAt);
  const updatedDate = new Date(order.updatedAt);
  const isRecentlyUpdated = Date.now() - updatedDate.getTime() < 5000;

  return (
    <div className={s.panel}>
      <div className={s.header}>
        <div className={s.headerTop}>
          <h2 className={s.title}>{order.orderNumber}</h2>
          <button className={s.closeButton} onClick={onClose} aria-label="Close">
            <X size={20} />
          </button>
        </div>
        <div className={s.headerBottom}>
          <OrderStatusBadge status={order.status} />
          <UpdateOrderStatus order={order} />
        </div>
      </div>

      <div className={s.content}>
        <Card title="Status Timeline">
          <StatusStepper currentStatus={order.status} />
        </Card>

        <Card title="Order Details">
          <div className={s.details}>
            <div className={s.detailRow}>
              <span className={s.detailLabel}>Description</span>
              <span className={s.detailValue}>{order.description}</span>
            </div>
            <div className={s.detailRow}>
              <span className={s.detailLabel}>Created</span>
              <span className={s.detailValue}>
                {createdDate.toLocaleDateString()} {createdDate.toLocaleTimeString()}
              </span>
            </div>
            {updatedDate.getTime() !== createdDate.getTime() && (
              <div className={s.detailRow}>
                <span className={s.detailLabel}>Last Updated</span>
                <span className={s.detailValue}>
                  {updatedDate.toLocaleDateString()} {updatedDate.toLocaleTimeString()}
                  {isRecentlyUpdated && (
                    <span className={s.pulse}>
                      <Circle size={8} fill="currentColor" />
                    </span>
                  )}
                </span>
              </div>
            )}
          </div>
        </Card>

        {activities.length > 0 && (
          <Card title="Activity">
            <ActivityFeed activities={activities} />
          </Card>
        )}
      </div>
    </div>
  );
}
