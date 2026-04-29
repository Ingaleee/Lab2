import { useState } from "react";

import { useCreateOrderMutation } from "../../entities/order/model/mutations";
import { Button } from "../../shared/ui/Button/Button";
import { Input } from "../../shared/ui/Input/Input";

import s from "./CreateOrderForm.module.css";

export function CreateOrderForm({ onSuccess }: { onSuccess?: () => void }) {
  const [orderNumber, setOrderNumber] = useState("");
  const [description, setDescription] = useState("");

  const createMutation = useCreateOrderMutation();

  const canSubmit = orderNumber.trim().length > 0 && description.trim().length > 0;

  const handleSubmit = () => {
    if (canSubmit) {
      createMutation.mutate(
        { orderNumber, description },
        {
          onSuccess: () => {
            setOrderNumber("");
            setDescription("");
            onSuccess?.();
          },
        }
      );
    }
  };

  return (
    <div className={s.form}>
      <div className={s.field}>
        <label className={s.label}>Order Number</label>
        <Input
          placeholder="Enter order number"
          value={orderNumber}
          onChange={(e) => setOrderNumber(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && handleSubmit()}
          autoFocus
        />
      </div>

      <div className={s.field}>
        <label className={s.label}>Description</label>
        <Input
          placeholder="Enter description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && handleSubmit()}
        />
      </div>

      <div className={s.actions}>
        <Button
          disabled={!canSubmit || createMutation.isPending}
          onClick={handleSubmit}
          className={s.submitButton}
        >
          {createMutation.isPending ? "Creating..." : "Create Order"}
        </Button>
      </div>
    </div>
  );
}
