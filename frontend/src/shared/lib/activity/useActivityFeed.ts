import { useState, useEffect, useRef } from "react";
import type {
  Order,
  OrderStatus,
  OrderStatusChangedIntegrationEventV1,
} from "../../../entities/order/model/types";
import { onOrderStatusChanged } from "../signalr/ordersHub";

export type ActivityItem = {
  id: string;
  type: "created" | "status_changed";
  timestamp: string;
  label: string;
  oldStatus?: OrderStatus;
  newStatus?: OrderStatus;
};

const STATUS_FLOW: OrderStatus[] = ["New", "InProgress", "Delivered"];

function inferStatusHistory(
  currentStatus: OrderStatus,
  createdAt: string,
  updatedAt: string,
  orderId: string
): ActivityItem[] {
  const activities: ActivityItem[] = [
    {
      id: `created-${orderId}`,
      type: "created",
      timestamp: createdAt,
      label: "Order created",
    },
  ];

  if (currentStatus === "New" || createdAt === updatedAt) {
    return activities;
  }

  if (currentStatus === "Cancelled") {
    activities.push({
      id: `cancelled-${orderId}-${updatedAt}`,
      type: "status_changed",
      timestamp: updatedAt,
      label: "Status changed to Cancelled",
      newStatus: "Cancelled",
    });
    return activities;
  }

  const currentIndex = STATUS_FLOW.indexOf(currentStatus);
  if (currentIndex <= 0) {
    return activities;
  }

  for (let i = 1; i <= currentIndex; i++) {
    const prevStatus = STATUS_FLOW[i - 1];
    const newStatus = STATUS_FLOW[i];
    
    activities.push({
      id: `inferred-${orderId}-${prevStatus}-${newStatus}`,
      type: "status_changed",
      timestamp: updatedAt,
      label: `Status changed: ${prevStatus} → ${newStatus}`,
      oldStatus: prevStatus,
      newStatus: newStatus,
    });
  }

  return activities;
}

export function useActivityFeed(orderId: string, order: Order | undefined) {
  const [activities, setActivities] = useState<ActivityItem[]>([]);
  const receivedEventsRef = useRef<Set<string>>(new Set());

  useEffect(() => {
    if (!order) return;

    const inferred = inferStatusHistory(order.status, order.createdAt, order.updatedAt, order.id);
    setActivities(inferred);
    receivedEventsRef.current.clear();
  }, [order?.id, order?.status, order?.createdAt, order?.updatedAt]);

  useEffect(() => {
    const unsubscribe = onOrderStatusChanged((evt: OrderStatusChangedIntegrationEventV1) => {
      if (evt.orderId === orderId) {
        setActivities((prev) => {
          if (receivedEventsRef.current.has(evt.eventId)) {
            return prev;
          }

          receivedEventsRef.current.add(evt.eventId);

          const existingIndex = prev.findIndex(
            (a) =>
              a.type === "status_changed" &&
              a.oldStatus === (evt.oldStatus as OrderStatus) &&
              a.newStatus === (evt.newStatus as OrderStatus) &&
              a.id.startsWith("inferred-")
          );

          if (existingIndex >= 0) {
            const updated = [...prev];
            updated[existingIndex] = {
              id: evt.eventId,
              type: "status_changed",
              timestamp: evt.occurredAt,
              label: `Status changed: ${evt.oldStatus} → ${evt.newStatus}`,
              oldStatus: evt.oldStatus as OrderStatus,
              newStatus: evt.newStatus as OrderStatus,
            };
            return updated;
          }

          const duplicateIndex = prev.findIndex(
            (a) =>
              a.type === "status_changed" &&
              a.oldStatus === (evt.oldStatus as OrderStatus) &&
              a.newStatus === (evt.newStatus as OrderStatus) &&
              !a.id.startsWith("inferred-")
          );

          if (duplicateIndex >= 0) {
            return prev;
          }

          return [
            ...prev,
            {
              id: evt.eventId,
              type: "status_changed",
              timestamp: evt.occurredAt,
              label: `Status changed: ${evt.oldStatus} → ${evt.newStatus}`,
              oldStatus: evt.oldStatus as OrderStatus,
              newStatus: evt.newStatus as OrderStatus,
            },
          ];
        });
      }
    });

    return unsubscribe;
  }, [orderId]);

  return activities.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
}
