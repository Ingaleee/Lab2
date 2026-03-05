import * as signalR from "@microsoft/signalr";
import { env } from "../../config/env";
import type { OrderStatusChangedIntegrationEventV1 } from "../../../entities/order/model/types";
import { HubMethods, HubEvents } from "./constants";

type ConnectionStatus = "disconnected" | "connecting" | "connected" | "reconnecting";

let connection: signalR.HubConnection | null = null;

export function getOrdersHubConnection() {
  if (connection) return connection;

  connection = new signalR.HubConnectionBuilder()
    .withUrl(env.signalrHubUrl, {
      skipNegotiation: false,
      withCredentials: true,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .configureLogging(signalR.LogLevel.Information)
    .build();

  return connection;
}

export function onOrderStatusChanged(handler: (evt: OrderStatusChangedIntegrationEventV1) => void) {
  const conn = getOrdersHubConnection();
  conn.on(HubEvents.OrderStatusChanged, handler);
  return () => conn.off(HubEvents.OrderStatusChanged, handler);
}

export async function startOrdersHub() {
  const conn = getOrdersHubConnection();
  
  if (conn.state === signalR.HubConnectionState.Connected) {
    return;
  }
  
  if (
    conn.state === signalR.HubConnectionState.Connecting ||
    conn.state === signalR.HubConnectionState.Reconnecting
  ) {
    return;
  }

  try {
    await conn.start();
    console.log("SignalR connected successfully");
  } catch (err) {
    const error = err instanceof Error ? err.message : String(err);
    console.error("SignalR start failed:", error);
    throw err;
  }
}

export async function stopOrdersHub() {
  const conn = getOrdersHubConnection();
  await conn.stop();
}

export async function joinOrdersList() {
  const conn = getOrdersHubConnection();
  await conn.invoke(HubMethods.JoinOrdersList);
}

export async function joinOrder(orderId: string) {
  const conn = getOrdersHubConnection();
  await conn.invoke(HubMethods.JoinOrder, orderId);
}

export async function leaveOrdersList() {
  const conn = getOrdersHubConnection();
  await conn.invoke(HubMethods.LeaveOrdersList);
}

export async function leaveOrder(orderId: string) {
  const conn = getOrdersHubConnection();
  await conn.invoke(HubMethods.LeaveOrder, orderId);
}

export function subscribeConnectionStatus(cb: (s: ConnectionStatus) => void) {
  const conn = getOrdersHubConnection();

  const emit = () => {
    const s =
      conn.state === signalR.HubConnectionState.Connected
        ? "connected"
        : conn.state === signalR.HubConnectionState.Connecting
          ? "connecting"
          : conn.state === signalR.HubConnectionState.Reconnecting
            ? "reconnecting"
            : "disconnected";
    cb(s);
  };

  emit();

  conn.onreconnecting(() => cb("reconnecting"));
  conn.onreconnected(() => {
    cb("connected");
  });
  conn.onclose(() => cb("disconnected"));

  return () => {
  };
}
