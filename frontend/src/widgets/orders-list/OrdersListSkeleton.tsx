import { Skeleton } from "../../shared/ui/Skeleton/Skeleton";
import s from "./OrdersListSkeleton.module.css";

export function OrdersListSkeleton() {
  return (
    <div className={s.container}>
      {Array.from({ length: 5 }).map((_, i) => (
        <div key={i} className={s.card}>
          <div className={s.header}>
            <Skeleton width="200px" height="20px" />
            <Skeleton width="100px" height="32px" />
          </div>
          <Skeleton width="100%" height="16px" className={s.description} />
          <Skeleton width="100%" height="16px" className={s.description} />
          <div className={s.meta}>
            <Skeleton width="150px" height="14px" />
          </div>
        </div>
      ))}
    </div>
  );
}
