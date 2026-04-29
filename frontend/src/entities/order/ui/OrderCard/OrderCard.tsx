import { useRef, useEffect } from "react";
import { useSearchParams } from "react-router-dom";

import { useNow } from "../../../../shared/lib/time/useNow";
import type { Order } from "../../model/types";
import { OrderStatusBadge } from "../OrderStatusBadge/OrderStatusBadge";

import s from "./OrderCard.module.css";
import { OrderCardQuickActions } from "./OrderCardQuickActions";

export function OrderCard({ order, highlight = false }: { order: Order; highlight?: boolean }) {
  const cardRef = useRef<HTMLDivElement>(null);
  const [, setSearchParams] = useSearchParams();

  useEffect(() => {
    if (highlight && cardRef.current) {
      cardRef.current.classList.add(s.highlight);
      const timer = setTimeout(() => {
        cardRef.current?.classList.remove(s.highlight);
      }, 800);
      return () => clearTimeout(timer);
    }
  }, [highlight]);

  const now = useNow();
  const createdDate = new Date(order.createdAt);
  const updatedDate = new Date(order.updatedAt);
  const isRecentlyUpdated = now - updatedDate.getTime() < 5000;

  return (
    <div ref={cardRef} className={s.card}>
      {isRecentlyUpdated && <div className={s.liveIndicator} title="Recently updated" />}
      <OrderCardQuickActions order={order} />
      <div className={s.row}>
        <div className={s.content}>
          <div className={s.title}>{order.orderNumber}</div>
          <div className={s.description}>{order.description}</div>
          <div className={s.meta}>
            <span className={s.metaItem}>
              Created: {createdDate.toLocaleDateString()} {createdDate.toLocaleTimeString()}
            </span>
            {updatedDate.getTime() !== createdDate.getTime() && (
              <>
                <span className={s.separator}>·</span>
                <span className={s.metaItem}>
                  Updated: {updatedDate.toLocaleDateString()} {updatedDate.toLocaleTimeString()}
                </span>
              </>
            )}
          </div>
        </div>
        <div className={s.side}>
          <OrderStatusBadge status={order.status} />
          <div className={s.sideActions}>
            <button className={s.link} onClick={() => setSearchParams({ orderId: order.id })}>
              Open
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
