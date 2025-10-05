// src/features/auth/AuthContext.tsx
import React, { createContext, useContext, useMemo, useState } from "react";
import { login as apiLogin, register as apiRegister } from "./api";
import type { AuthUser, AuthResponse } from "./types";
import {
  setStoredToken,
  getStoredToken,
  setStoredUser,
  getStoredUser,
  type StoredUser,
} from "../../api/apiClient";
import { toast } from "sonner";

type AuthContextValue = {
  token: string | null;
  user: AuthUser | null;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, fullName?: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(() => getStoredToken());
  const [user, setUser] = useState<AuthUser | null>(() => getStoredUser() as StoredUser | null);

  function handleAuth(res: AuthResponse) {
    setToken(res.accessToken);
    setUser(res.user);
    setStoredToken(res.accessToken);
    setStoredUser(res.user);
  }

  const value = useMemo<AuthContextValue>(
    () => ({
      token,
      user,

      async login(email: string, password: string) {
        try {
          const res = await apiLogin(email, password);
          handleAuth(res);
          toast.success("Logged in successfully!");
        } catch (e: unknown) {
          const err = e as { response?: { data?: { detail?: string; message?: string } } };
          const msg = err?.response?.data?.detail || err?.response?.data?.message || "Invalid email or password";
          toast.error(msg);
          throw e;
        }
      },

      async register(email: string, password: string, fullName?: string) {
        try {
          const res = await apiRegister(email, password, fullName);
          toast.success("Registered successfully! Please log in.");

          // Clear stored session and redirect
          setToken(null);
          setUser(null);
          setStoredToken(null);
          setStoredUser(null);

          // Redirect to login
          window.location.href = "/login";
        } catch (e: unknown) {
          const err = e as { response?: { data?: { detail?: string; message?: string } } };
          const msg = err?.response?.data?.detail || err?.response?.data?.message || "Registration failed";
          toast.error(msg);
          throw e;
        }
      },

      logout() {
        setToken(null);
        setUser(null);
        setStoredToken(null);
        setStoredUser(null);
        toast.success("Logged out successfully!");
      },
    }),
    [token, user]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
