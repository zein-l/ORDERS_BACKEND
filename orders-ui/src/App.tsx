import type { FC, PropsWithChildren } from "react";
import { Routes, Route, Navigate } from "react-router-dom";

import Header from "./components/Header";
import { useAuth } from "./features/auth/AuthContext";

import LoginPage from "./features/auth/LoginPage";
import RegisterPage from "./features/auth/RegisterPage";
import OrdersPage from "./features/orders/OrdersPage";

const Protected: FC<PropsWithChildren> = ({ children }) => {
  const { token } = useAuth();
  if (!token) return <Navigate to="/login" replace />;
  return <>{children}</>;
};

export default function App() {
  return (
    <div className="min-h-dvh bg-[#0b0f14] text-white/90">
      <Header />

      <main className="mx-auto max-w-6xl px-4 py-8">
        <Routes>
          {/* default: send users to /orders if signed in, otherwise /login */}
          <Route path="/" element={<HomeRedirect />} />

          <Route
            path="/orders"
            element={
              <Protected>
                <OrdersPage />
              </Protected>
            }
          />

          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* catch-all */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
    </div>
  );
}

function HomeRedirect() {
  const { token } = useAuth();
  return <Navigate to={token ? "/orders" : "/login"} replace />;
}
