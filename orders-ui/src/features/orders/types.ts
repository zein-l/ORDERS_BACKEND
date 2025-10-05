// src/features/orders/types.ts
export type OrderItemResponse = {
  id: string;
  name: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
};

export type OrderResponse = {
  id: string;
  userId: string;
  status: string;
  total: number;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
  items: OrderItemResponse[];
};

export type AddItemRequest = {
  name: string;
  quantity: number;
  unitPrice: number;
};
