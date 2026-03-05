import { Check, X } from "lucide-react";
import type { OrderStatus } from "../../../entities/order/model/types";
import s from "./StatusStepper.module.css";

const statusOrder: OrderStatus[] = ["New", "InProgress", "Delivered"];
const cancelledStatus: OrderStatus = "Cancelled";

export function StatusStepper({ currentStatus }: { currentStatus: OrderStatus }) {
  const isCancelled = currentStatus === cancelledStatus;
  const currentIndex = isCancelled ? -1 : statusOrder.indexOf(currentStatus);

  return (
    <div className={s.stepper}>
      {statusOrder.map((status, index) => {
        const isActive = index <= currentIndex;
        const isCurrent = index === currentIndex;

        return (
          <div key={status} className={s.step}>
            <div className={`${s.stepCircle} ${isActive ? s.active : ""} ${isCurrent ? s.current : ""}`}>
              {isActive ? <Check size={14} /> : index + 1}
            </div>
            <div className={`${s.stepLabel} ${isActive ? s.active : ""} ${isCurrent ? s.current : ""}`}>
              {status}
            </div>
            {index < statusOrder.length - 1 && (
              <div className={`${s.stepLine} ${isActive ? s.active : ""}`} />
            )}
          </div>
        );
      })}
      {isCancelled && (
        <div className={s.cancelledStep}>
          <div className={`${s.stepCircle} ${s.cancelled}`}>
            <X size={14} />
          </div>
          <div className={`${s.stepLabel} ${s.cancelled}`}>Cancelled</div>
        </div>
      )}
    </div>
  );
}
