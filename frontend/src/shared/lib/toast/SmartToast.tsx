import React from "react";
import toast from "react-hot-toast";
import { Package } from "lucide-react";
import type { OrderStatusChangedIntegrationEventV1 } from "../../../entities/order/model/types";
import s from "./SmartToast.module.css";

let toastQueue: OrderStatusChangedIntegrationEventV1[] = [];
let toastTimer: ReturnType<typeof setTimeout> | null = null;
const TOAST_GROUP_DELAY = 1000;

function ToastContent({
  message,
  orderId,
}: {
  message: string;
  orderId?: string;
}) {
  const handleClick = () => {
    if (orderId) {
      window.location.href = `/orders?orderId=${orderId}`;
    }
  };

  return (
    <div className={s.container}>
      <div className={s.message}>{message}</div>
      {orderId && (
        <button onClick={handleClick} className={s.button}>
          Open
        </button>
      )}
    </div>
  );
}

export function showSmartToast(evt: OrderStatusChangedIntegrationEventV1) {
  try {
    toastQueue.push(evt);

    if (toastTimer) {
      clearTimeout(toastTimer);
    }

    toastTimer = setTimeout(() => {
      try {
        if (toastQueue.length === 1) {
          const single = toastQueue[0];
          const orderIdShort = single.orderId.slice(0, 8);
          
          toast.custom(
            (t) => (
              <ToastContent
                message={`Order #${orderIdShort} moved to ${single.newStatus}`}
                orderId={single.orderId}
              />
            ),
            {
              duration: 5000,
              icon: <Package size={16} />,
            }
          );
        } else {
          const latest = toastQueue[toastQueue.length - 1];
          toast.custom(
            (t) => (
              <ToastContent
                message={`${toastQueue.length} updates received`}
                orderId={latest.orderId}
              />
            ),
            {
              duration: 5000,
              icon: <Package size={16} />,
            }
          );
        }

        toastQueue = [];
        toastTimer = null;
      } catch (error) {
        console.error("Error showing toast:", error);
        toastQueue = [];
        toastTimer = null;
      }
    }, TOAST_GROUP_DELAY);
  } catch (error) {
    console.error("Error in showSmartToast:", error);
  }
}
