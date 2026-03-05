export const orderKeys = {
  all: ["orders"] as const,
  list: () => [...orderKeys.all, "list"] as const,
  byId: (id: string) => [...orderKeys.all, "byId", id] as const,
};
