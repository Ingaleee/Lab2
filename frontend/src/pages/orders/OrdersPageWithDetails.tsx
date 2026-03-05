import { useState } from "react";
import { useSearchParams } from "react-router-dom";
import { CreateOrderForm } from "../../features/order-create/CreateOrderForm";
import { OrdersList } from "../../widgets/orders-list/OrdersList";
import { OrderDetailsPanel } from "../../widgets/order-details/OrderDetailsPanel";
import { SidePanel } from "../../shared/ui/SidePanel/SidePanel";
import { CommandPalette } from "../../shared/ui/CommandPalette/CommandPalette";
import { useKeyboardShortcuts } from "../../shared/lib/keyboard/useKeyboardShortcuts";
import { useCommandPalette } from "../../shared/lib/commandPalette/CommandPaletteContext";
import { useOrdersQuery } from "../../entities/order/model/queries";
import { Button } from "../../shared/ui/Button/Button";
import s from "./OrdersPageWithDetails.module.css";

export function OrdersPageWithDetails() {
  const [isCreatePanelOpen, setIsCreatePanelOpen] = useState(false);
  const { isOpen: isCommandPaletteOpen, open: openCommandPalette, close: closeCommandPalette } = useCommandPalette();
  const [searchParams, setSearchParams] = useSearchParams();
  const selectedOrderId = searchParams.get("orderId");
  const ordersQuery = useOrdersQuery();
  const ordersCount = ordersQuery.data?.length ?? 0;

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
        if (selectedOrderId) setSearchParams({});
        closeCommandPalette();
      },
    },
  ]);

  return (
    <div className={s.page}>
      <div className={s.stickyHeader}>
        <div className={s.headerContent}>
          <div className={s.pageTitle}>
            <h1 className={s.title}>Orders</h1>
            {ordersCount > 0 && (
              <span className={s.count}>{ordersCount} {ordersCount === 1 ? "order" : "orders"}</span>
            )}
          </div>
          <Button onClick={() => setIsCreatePanelOpen(true)} className={s.createButton}>
            Create order
          </Button>
        </div>
      </div>

      <div className={`${s.layout} ${selectedOrderId ? s.withDetails : ""}`}>
        <div className={s.listSection}>
          <OrdersList onCreateOrder={() => setIsCreatePanelOpen(true)} />
        </div>

        {selectedOrderId && (
          <div className={s.detailsSection}>
            <OrderDetailsPanel
              orderId={selectedOrderId}
              onClose={() => setSearchParams({})}
            />
          </div>
        )}
      </div>

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
