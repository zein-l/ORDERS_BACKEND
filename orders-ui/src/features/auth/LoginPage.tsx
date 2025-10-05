import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useAuth } from "./AuthContext";
import { useNavigate, Link } from "react-router-dom";

const schema = z.object({
  email: z.string().email(),
  password: z.string().min(6),
});
type FormData = z.infer<typeof schema>;

export default function LoginPage() {
  const { login } = useAuth();
  const nav = useNavigate();
  const { register, handleSubmit, formState: { errors, isSubmitting } } =
    useForm<FormData>({ resolver: zodResolver(schema) });

  async function onSubmit(values: FormData) {
    await login(values.email, values.password);
    nav("/orders");
  }

  return (
    <div className="mx-auto max-w-md p-4">
      <div className="rounded-2xl border border-white/10 bg-white/5 p-6 shadow-xl">
        <h1 className="mb-6 text-2xl font-semibold">Login</h1>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
          <div>
            <label className="mb-1 block text-sm text-white/70">Email</label>
            <input
              type="email"
              {...register("email")}
              className="w-full rounded-lg border border-white/10 bg-black/30 px-3 py-2 outline-none focus:border-white/30"
            />
            {errors.email && (
              <p className="mt-1 text-xs text-red-400">{errors.email.message}</p>
            )}
          </div>

          <div>
            <label className="mb-1 block text-sm text-white/70">Password</label>
            <input
              type="password"
              {...register("password")}
              className="w-full rounded-lg border border-white/10 bg-black/30 px-3 py-2 outline-none focus:border-white/30"
            />
            {errors.password && (
              <p className="mt-1 text-xs text-red-400">{errors.password.message}</p>
            )}
          </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="w-full rounded-lg bg-indigo-600 px-4 py-2 font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
        >
          {isSubmitting ? "Signing in…" : "Sign in"}
        </button>
        </form>

        <p className="mt-4 text-sm text-white/70">
          No account?{" "}
          <Link to="/register" className="text-indigo-300 hover:underline">
            Register
          </Link>
        </p>
      </div>
    </div>
  );
}
