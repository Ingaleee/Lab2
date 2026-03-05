export const HubMethods = {
  JoinOrdersList: "JoinOrdersList",
  LeaveOrdersList: "LeaveOrdersList",
  JoinOrder: "JoinOrder",
  LeaveOrder: "LeaveOrder",
} as const;

export const HubEvents = {
  OrderStatusChanged: "orderStatusChanged",
} as const;
