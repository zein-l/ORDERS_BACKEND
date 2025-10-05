import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../features/auth/AuthContext";

export default function Header() {
  const { token, logout } = useAuth();
  const nav = useNavigate();

  function handleLogout() {
    logout();
    nav("/login");
  }

  return (
    <header className="sticky top-0 z-10 border-b border-white/10 bg-black/30 backdrop-blur">
      <div className="mx-auto flex h-14 max-w-6xl items-center justify-between px-4">
        {/* NOT a link anymore */}
        <span className="select-none text-lg font-semibold tracking-wide text-white/90">
          Orders
        </span>

        <nav className="flex items-center gap-2">
          {!token ? (
            <>
              <Link
                to="/login"
                className="rounded-md px-3 py-1 text-sm text-white/80 hover:text-white"
              >
                Login
              </Link>
              <Link
                to="/register"
                className="rounded-md border border-indigo-500/40 bg-indigo-600/80 px-3 py-1 text-sm text-white hover:bg-indigo-600"
              >
                Register
              </Link>
            </>
          ) : (
            <button
              onClick={handleLogout}
              className="rounded-md border border-white/10 bg-white/10 px-3 py-1 text-sm text-white hover:bg-white/20"
            >
              Logout
            </button>
          )}
        </nav>
      </div>
    </header>
  );
}
