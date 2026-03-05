import { useMutation } from "@tanstack/react-query";
import { ordersApi } from "../api/ordersApi";
import type { CreateOrderRequest, UpdateOrderStatusRequest } from "../api/ordersApi";
import { queryClient } from "../../../shared/lib/reactQuery/queryClient";
import { orderKeys } from "./queryKeys";
import type { Order, OrderStatus } from "./types";
import toast from "react-hot-toast";
import { getErrorMessage } from "../../../shared/api/errors";

export function useCreateOrderMutation() {
  return useMutation({
    mutationFn: (payload: CreateOrderRequest) => ordersApi.createOrder(payload),
    onSuccess: (created) => {
      queryClient.setQueryData<Order[]>(orderKeys.list(), (old) => {
        if (!old) return [created];
        return [created, ...old];
      });
      toast.success("Order created");
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
  });
}

export function useUpdateOrderStatusMutation(orderId: string) {
  return useMutation({
    mutationFn: (payload: UpdateOrderStatusRequest) => ordersApi.updateStatus(orderId, payload),

    onMutate: async (payload) => {
      await queryClient.cancelQueries({ queryKey: orderKeys.byId(orderId) });
      await queryClient.cancelQueries({ queryKey: orderKeys.list() });

      const prevOrder = queryClient.getQueryData<Order>(orderKeys.byId(orderId));
      const prevList = queryClient.getQueryData<Order[]>(orderKeys.list());

      if (prevOrder) {
        queryClient.setQueryData<Order>(orderKeys.byId(orderId), {
          ...prevOrder,
          status: payload.status as OrderStatus,
          updatedAt: new Date().toISOString(),
        });
      }

      if (prevList) {
        queryClient.setQueryData<Order[]>(
          orderKeys.list(),
          prevList.map((o) =>
            o.id === orderId
              ? { ...o, status: payload.status as OrderStatus, updatedAt: new Date().toISOString() }
              : o
          )
        );
      }

      return { prevOrder, prevList };
    },

    onError: (err, _payload, ctx) => {
      if (ctx?.prevOrder) queryClient.setQueryData(orderKeys.byId(orderId), ctx.prevOrder);
      if (ctx?.prevList) queryClient.setQueryData(orderKeys.list(), ctx.prevList);

      toast.error(getErrorMessage(err));
    },

    onSuccess: (updated) => {
      try {
        queryClient.setQueryData(orderKeys.byId(orderId), updated);
        queryClient.setQueryData<Order[]>(orderKeys.list(), (old) => {
          if (!old) return old;
          return old.map((o) => (o.id === updated.id ? updated : o));
        });

        toast.success("Status updated");
      } catch (error) {
        console.error("Error updating query cache:", error);
      }
    },

    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: orderKeys.byId(orderId) }).catch(console.error);
      queryClient.invalidateQueries({ queryKey: orderKeys.list() }).catch(console.error);
    },
  });
}

export function useDeleteOrderMutation() {
  return useMutation({
    mutationFn: (id: string) => ordersApi.deleteOrder(id),
    onSuccess: (_, id) => {
      queryClient.setQueryData<Order[]>(orderKeys.list(), (old) => old?.filter((o) => o.id !== id));
      queryClient.removeQueries({ queryKey: orderKeys.byId(id) });
    },
  });
}
