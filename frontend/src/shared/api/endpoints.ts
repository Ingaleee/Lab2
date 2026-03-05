export const ApiEndpoints = {
  Orders: {
    List: "/api/orders",
    ById: (id: string) => `/api/orders/${id}`,
    Create: "/api/orders",
    UpdateStatus: (id: string) => `/api/orders/${id}/status`,
    Delete: (id: string) => `/api/orders/${id}`,
  },
} as const;
