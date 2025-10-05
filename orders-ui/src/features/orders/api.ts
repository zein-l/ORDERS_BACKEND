// src/features/orders/api.ts
import { api } from "../../api/apiClient";
import type { OrderResponse, AddItemRequest } from "./types";

export async function getMyOrders(): Promise<OrderResponse[]> {
  const { data } = await api.get<OrderResponse[]>("/api/me/orders");
  return data;
}

export async function getOrderById(orderId: string): Promise<OrderResponse> {
  const { data } = await api.get<OrderResponse>(`/api/orders/${orderId}`);
  return data;
}

export async function createOrder(): Promise<OrderResponse> {
  const { data } = await api.post<OrderResponse>("/api/orders");
  return data;
}

export async function addItem(orderId: string, body: AddItemRequest): Promise<OrderResponse> {
  const { data } = await api.post<OrderResponse>(`/api/orders/${orderId}/items`, body);
  return data;
}

export async function removeItem(orderId: string, itemId: string): Promise<OrderResponse> {
  const { data } = await api.delete<OrderResponse>(`/api/orders/${orderId}/items/${itemId}`);
  return data;
}
