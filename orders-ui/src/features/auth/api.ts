import { api } from "../../api/apiClient";
import type { AuthResponse } from "./types";

export async function login(email: string, password: string): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>("/api/auth/login", { email, password });
  return data;
}

export async function register(email: string, password: string, fullName?: string): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>("/api/auth/register", { email, password, fullName });
  return data;
}
