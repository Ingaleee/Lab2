import { useEffect, useRef, useState } from "react";
import type { ReactNode } from "react";
import toast, { Toaster } from "react-hot-toast";
import { queryClient } from "../../shared/lib/reactQuery/queryClient";
import { orderKeys } from "../../entities/order/model/queryKeys";
import type { Order, OrderStatusChangedIntegrationEventV1 } from "../../entities/order/model/types";
import {
  joinOrdersList,
  onOrderStatusChanged,
  startOrdersHub,
  subscribeConnectionStatus,
} from "../../shared/lib/signalr/ordersHub";
import { LruSet } from "../../shared/lib/signalr/eventDedup";
import { RealtimeContext, type ConnectionStatus } from "../../shared/lib/realtime/RealtimeContext";
import {
  RealtimeUpdateProvider,
  useRealtimeUpdates,
} from "../../shared/lib/realtime/RealtimeUpdateContext";
import { showSmartToast } from "../../shared/lib/toast/SmartToast";

const dedup = new LruSet(500);

function RealtimeConnection({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<ConnectionStatus>("disconnected");
  const wasConnectedOnceRef = useRef(false);
  const wasReconnectingRef = useRef(false);
  const { markHighlighted } = useRealtimeUpdates();

  useEffect(() => {
    subscribeConnectionStatus((s) => {
      setStatus(s);

      if (s === "connected") {
        if (wasReconnectingRef.current) {
          toast.success("Reconnected. Syncing…");
          queryClient.invalidateQueries({ queryKey: orderKeys.list() });
          wasReconnectingRef.current = false;
        } else if (!wasConnectedOnceRef.current) {
          wasConnectedOnceRef.current = true;
        }
      }

      if (s === "reconnecting") {
        wasReconnectingRef.current = true;
        toast("Reconnecting…");
      }

      if (s === "disconnected" && wasConnectedOnceRef.current) {
        toast.error("Realtime offline. Still works via polling.");
      }
    });
  }, []);

  useEffect(() => {
    let unsub: (() => void) | null = null;

    function handleOrderStatusChanged(evt: OrderStatusChangedIntegrationEventV1) {
      if (!evt?.eventId) return;

      if (dedup.has(evt.eventId)) return;
      dedup.add(evt.eventId);

      markHighlighted(evt.orderId);
      showSmartToast(evt);

      queryClient.setQueryData<Order>(orderKeys.byId(evt.orderId), (old) => {
        if (!old) return undefined;
        return {
          ...old,
          status: evt.newStatus,
          updatedAt: evt.occurredAt,
        };
      });

      queryClient.setQueryData<Order[]>(orderKeys.list(), (old) => {
        if (!old) return old;
        return old.map((o) =>
          o.id === evt.orderId
            ? {
                ...o,
                status: evt.newStatus,
                updatedAt: evt.occurredAt,
              }
            : o
        );
      });
    }

    (async () => {
      try {
        setStatus("connecting");
        await startOrdersHub();
        await joinOrdersList();

        unsub = onOrderStatusChanged((evt) => {
          handleOrderStatusChanged(evt);
        });

        setStatus("connected");
        wasConnectedOnceRef.current = true;
      } catch (e) {
        setStatus("disconnected");
        const error = e instanceof Error ? e.message : String(e);
        console.warn("SignalR connection failed:", error);
        
        if (wasConnectedOnceRef.current) {
          toast.error("Realtime connection lost (will retry)");
        }
      }
    })();

    return () => {
      if (unsub) unsub();
    };
  }, [markHighlighted]);

  return (
    <RealtimeContext.Provider value={status}>
      {children}
    </RealtimeContext.Provider>
  );
}

export function RealtimeProvider({ children }: { children: ReactNode }) {
  return (
    <RealtimeUpdateProvider>
      <RealtimeConnection>
        <Toaster position="top-right" />
        {children}
      </RealtimeConnection>
    </RealtimeUpdateProvider>
  );
}