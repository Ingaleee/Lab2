import type { ActivityItem } from "../../lib/activity/useActivityFeed";
import { OrderStatusBadge } from "../../../entities/order/ui/OrderStatusBadge/OrderStatusBadge";
import s from "./ActivityFeed.module.css";

export function ActivityFeed({ activities }: { activities: ActivityItem[] }) {
  if (activities.length === 0) return null;

  return (
    <div className={s.container}>
      <div className={s.title}>Activity</div>
      <div className={s.list}>
        {activities.map((activity) => (
          <div key={activity.id} className={s.item}>
            <div className={s.timeline}>
              <div className={s.dot} />
              {activity !== activities[activities.length - 1] && <div className={s.line} />}
            </div>
            <div className={s.content}>
              <div className={s.label}>{activity.label}</div>
              {activity.type === "status_changed" && activity.oldStatus && activity.newStatus && (
                <div className={s.statusChange}>
                  <OrderStatusBadge status={activity.oldStatus} />
                  <span className={s.arrow}>→</span>
                  <OrderStatusBadge status={activity.newStatus} />
                </div>
              )}
              <div className={s.timestamp}>
                {new Date(activity.timestamp).toLocaleString()}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
