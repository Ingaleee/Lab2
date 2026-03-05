import { createContext, useContext } from "react";

export type ConnectionStatus = "disconnected" | "connecting" | "connected" | "reconnecting";

export const RealtimeContext = createContext<ConnectionStatus>("disconnected");

export function useRealtimeStatus() {
  return useContext(RealtimeContext);
}
