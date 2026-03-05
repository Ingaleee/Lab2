import { createBrowserRouter, Navigate, Outlet } from "react-router-dom";
import { OrdersPageWithDetails } from "../../pages/orders/OrdersPageWithDetails";
import { OrderDetailsPage } from "../../pages/order-details/OrderDetailsPage";
import { AppHeader } from "../../shared/ui/AppHeader/AppHeader";

function Layout() {
  return (
    <div className="container">
      <AppHeader />
      <Outlet />
    </div>
  );
}

export const router = createBrowserRouter([
  {
    element: <Layout />,
    children: [
      { path: "/", element: <Navigate to="/orders" replace /> },
      { path: "/orders", element: <OrdersPageWithDetails /> },
      { path: "/orders/:id", element: <OrderDetailsPage /> },
    ],
  },
]);
