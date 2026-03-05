import { useQuery } from "@tanstack/react-query";
import { ordersApi } from "../api/ordersApi";
import { orderKeys } from "./queryKeys";

export function useOrdersQuery() {
  return useQuery({
    queryKey: orderKeys.list(),
    queryFn: ordersApi.getOrders,
  });
}

export function useOrderByIdQuery(id: string) {
  return useQuery({
    queryKey: orderKeys.byId(id),
    queryFn: () => ordersApi.getOrderById(id),
    enabled: Boolean(id),
  });
}
