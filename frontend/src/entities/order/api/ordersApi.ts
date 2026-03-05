import { http } from "../../../shared/api/http";
import { ApiEndpoints } from "../../../shared/api/endpoints";
import type { Order } from "../model/types";

export type CreateOrderRequest = {
  orderNumber: string;
  description: string;
};

export type UpdateOrderStatusRequest = {
  status: string;
};

export const ordersApi = {
  async getOrders(): Promise<Order[]> {
    const res = await http.get<Order[]>(ApiEndpoints.Orders.List);
    return res.data;
  },

  async getOrderById(id: string): Promise<Order> {
    const res = await http.get<Order>(ApiEndpoints.Orders.ById(id));
    return res.data;
  },

  async createOrder(payload: CreateOrderRequest): Promise<Order> {
    const res = await http.post<Order>(ApiEndpoints.Orders.Create, payload);
    return res.data;
  },

  async updateStatus(id: string, payload: UpdateOrderStatusRequest): Promise<Order> {
    const res = await http.patch<Order>(ApiEndpoints.Orders.UpdateStatus(id), payload);
    return res.data;
  },

  async deleteOrder(id: string): Promise<void> {
    await http.delete(ApiEndpoints.Orders.Delete(id));
  },
};
