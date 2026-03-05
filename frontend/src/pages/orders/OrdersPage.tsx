import { useState } from "react";
import { CreateOrderForm } from "../../features/order-create/CreateOrderForm";
import { OrdersList } from "../../widgets/orders-list/OrdersList";
import { SidePanel } from "../../shared/ui/SidePanel/SidePanel";
import { Button } from "../../shared/ui/Button/Button";
import { CommandPalette } from "../../shared/ui/CommandPalette/CommandPalette";
import { useKeyboardShortcuts } from "../../shared/lib/keyboard/useKeyboardShortcuts";
import { useCommandPalette } from "../../shared/lib/commandPalette/CommandPaletteContext";
import s from "./OrdersPage.module.css";

export function OrdersPage() {
  const [isCreatePanelOpen, setIsCreatePanelOpen] = useState(false);
  const { isOpen: isCommandPaletteOpen, open: openCommandPalette, close: closeCommandPalette } = useCommandPalette();

  useKeyboardShortcuts([
    {
      key: "k",
      ctrl: true,
      handler: () => openCommandPalette(),
    },
    {
      key: "Escape",
      handler: () => {
        if (isCreatePanelOpen) setIsCreatePanelOpen(false);
        closeCommandPalette();
      },
    },
  ]);

  return (
    <div className={s.page}>
      <div className={s.header}>
        <h1 className={s.title}>Orders</h1>
        <Button onClick={() => setIsCreatePanelOpen(true)}>Create Order</Button>
      </div>

      <OrdersList />

      <SidePanel
        isOpen={isCreatePanelOpen}
        onClose={() => setIsCreatePanelOpen(false)}
        title="Create Order"
      >
        <CreateOrderForm onSuccess={() => setIsCreatePanelOpen(false)} />
      </SidePanel>

      <CommandPalette
        isOpen={isCommandPaletteOpen}
        onClose={closeCommandPalette}
        onCreateOrder={() => {
          setIsCreatePanelOpen(true);
          closeCommandPalette();
        }}
      />
    </div>
  );
}
