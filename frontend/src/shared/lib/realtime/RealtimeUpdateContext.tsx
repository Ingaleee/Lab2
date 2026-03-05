import { createContext, useContext, useState, useEffect, useRef } from "react";

type HighlightedOrderIds = Set<string>;

export const RealtimeUpdateContext = createContext<{
  highlightedIds: HighlightedOrderIds;
  markHighlighted: (orderId: string) => void;
}>({
  highlightedIds: new Set(),
  markHighlighted: () => {},
});

export function useRealtimeUpdates() {
  return useContext(RealtimeUpdateContext);
}

export function RealtimeUpdateProvider({ children }: { children: React.ReactNode }) {
  const [highlightedIds, setHighlightedIds] = useState<HighlightedOrderIds>(new Set());

  const markHighlighted = (orderId: string) => {
    setHighlightedIds((prev) => new Set(prev).add(orderId));
    setTimeout(() => {
      setHighlightedIds((prev) => {
        const next = new Set(prev);
        next.delete(orderId);
        return next;
      });
    }, 800);
  };

  return (
    <RealtimeUpdateContext.Provider value={{ highlightedIds, markHighlighted }}>
      {children}
    </RealtimeUpdateContext.Provider>
  );
}
