import { useMemo, useState, useRef } from "react";
import { Package } from "lucide-react";
import { OrderCard } from "../../entities/order/ui/OrderCard/OrderCard";
import { useOrdersQuery } from "../../entities/order/model/queries";
import type { OrdersListFilter } from "../../entities/order/model/selectors";
import { filterAndSortOrders } from "../../entities/order/model/selectors";
import { OrdersListToolbar } from "./OrdersListToolbar";
import { OrdersListSkeleton } from "./OrdersListSkeleton";
import { StatusDistribution } from "./StatusDistribution";
import { useRealtimeUpdates } from "../../shared/lib/realtime/RealtimeUpdateContext";
import s from "./OrdersList.module.css";

export function OrdersList({ onCreateOrder }: { onCreateOrder?: () => void }) {
  const q = useOrdersQuery();
  const { highlightedIds } = useRealtimeUpdates();
  const searchInputRef = useRef<HTMLInputElement | null>(null);

  const [filter, setFilter] = useState<OrdersListFilter>({
    search: "",
    status: "All",
    sort: "Newest",
  });

  const filtered = useMemo(() => {
    if (!q.data) return [];
    return filterAndSortOrders(q.data, filter);
  }, [q.data, filter]);

  if (q.isLoading) return <OrdersListSkeleton />;
  if (q.isError) return <div className={s.error}>Failed to load orders</div>;

  const all = q.data ?? [];

  return (
    <div className={s.container}>
      <OrdersListToolbar value={filter} onChange={setFilter} searchInputRef={searchInputRef} />

      {all.length > 0 && <StatusDistribution orders={all} />}

      {all.length === 0 ? (
        <EmptyState
          title="No orders yet"
          subtitle="Create your first order to start tracking."
          onCreateOrder={onCreateOrder}
        />
      ) : filtered.length === 0 ? (
        <EmptyState
          title="No matches"
          subtitle="Try changing search or filters."
          onClearFilters={() => setFilter({ search: "", status: "All", sort: "Newest" })}
        />
      ) : (
        <div className={s.list}>
          {filtered.map((o) => (
            <OrderCard key={o.id} order={o} highlight={highlightedIds.has(o.id)} />
          ))}
        </div>
      )}
    </div>
  );
}

function EmptyState({
  title,
  subtitle,
  onCreateOrder,
  onClearFilters,
}: {
  title: string;
  subtitle: string;
  onCreateOrder?: () => void;
  onClearFilters?: () => void;
}) {
  return (
    <div className={s.emptyState}>
      <Package className={s.emptyStateIcon} size={48} />
      <div className={s.emptyStateTitle}>{title}</div>
      <div className={s.emptyStateSubtitle}>{subtitle}</div>
      {onCreateOrder && (
        <button className={s.emptyStateAction} onClick={onCreateOrder}>
          Create order
        </button>
      )}
      {onClearFilters && (
        <button className={s.emptyStateAction} onClick={onClearFilters}>
          Clear filters
        </button>
      )}
    </div>
  );
}
