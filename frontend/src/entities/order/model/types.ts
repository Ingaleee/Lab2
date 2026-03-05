export type OrderStatus = "New" | "InProgress" | "Delivered" | "Cancelled";

export type Order = {
  id: string;
  orderNumber: string;
  description: string;
  status: OrderStatus;
  createdAt: string;
  updatedAt: string;
};

export type OrderStatusChangedIntegrationEventV1 = {
  eventId: string;
  occurredAt: string;
  orderId: string;
  orderNumber: string;
  oldStatus: OrderStatus;
  newStatus: OrderStatus;
};
