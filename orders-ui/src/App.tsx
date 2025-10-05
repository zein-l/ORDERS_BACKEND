import type { JSX } from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import { Toaster } from "sonner";

import { useAuth } from "./features/auth/AuthContext";
import Header from "./components/Header";
import LoginPage from "./features/auth/LoginPage";
import RegisterPage from "./features/auth/RegisterPage";
import OrdersPage from "./features/orders/OrdersPage";

// Guard for private routes
function ProtectedRoute({ children }: { children: JSX.Element }) {
  const { token } = useAuth();
  return token ? children : <Navigate to="/login" replace />;
}

export default function App() {
  return (
    <div className="min-h-full bg-[#0b0f14] text-gray-200">
      <Header />

      <main className="mx-auto max-w-5xl px-4 py-8">
        <Routes>
          {/* Default: send logged in users to /orders, others to /login */}
          <Route path="/" element={<RedirectHome />} />

          {/* Public */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Private */}
          <Route
            path="/orders"
            element={
              <ProtectedRoute>
                <OrdersPage />
              </ProtectedRoute>
            }
          />

          {/* Fallback */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>

      {/* Toasts mounted once */}
      <Toaster richColors closeButton />
    </div>
  );
}

function RedirectHome() {
  const { token } = useAuth();
  return <Navigate to={token ? "/orders" : "/login"} replace />;
}
