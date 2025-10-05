import axios from "axios";

const baseURL = import.meta.env.VITE_API_BASE ?? "http://localhost:5238";

export const api = axios.create({ baseURL });

const TOKEN_KEY = "orders.token";
const USER_KEY = "orders.user";

export function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}
export function setStoredToken(token: string | null): void {
  if (token) localStorage.setItem(TOKEN_KEY, token);
  else localStorage.removeItem(TOKEN_KEY);
}

export type StoredUser = {
  id: string;
  email: string;
  fullName?: string | null;
};

export function getStoredUser(): StoredUser | null {
  const raw = localStorage.getItem(USER_KEY);
  return raw ? (JSON.parse(raw) as StoredUser) : null;
}
export function setStoredUser(user: StoredUser | null): void {
  if (user) localStorage.setItem(USER_KEY, JSON.stringify(user));
  else localStorage.removeItem(USER_KEY);
}

// Attach token
api.interceptors.request.use((config) => {
  const token = getStoredToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Handle 401s, but **skip** auth endpoints so toasts can show their errors
api.interceptors.response.use(
  (r) => r,
  (err) => {
    const response = err?.response;
    const url: string | undefined = err?.config?.url;
    const isAuthCall = url?.startsWith("/api/auth/"); // /api/auth/login or /api/auth/register

    if (response?.status === 401 && !isAuthCall) {
      setStoredToken(null);
      setStoredUser(null);
      if (location.pathname !== "/login") location.href = "/login";
    }
    return Promise.reject(err);
  }
);
