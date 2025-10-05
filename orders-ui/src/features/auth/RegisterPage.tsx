import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "./AuthContext";

const schema = z.object({
  fullName: z.string().trim().min(1, "Full name is required"),
  email: z.string().email("Invalid email address"),
  password: z.string().min(6, "Minimum 6 characters"),
});
type FormData = z.infer<typeof schema>;

export default function RegisterPage() {
  const { register: doRegister } = useAuth();
  const nav = useNavigate();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  async function onSubmit(values: FormData) {
    await doRegister(values.email, values.password, values.fullName);
    nav("/orders");
  }

  return (
    <div className="mx-auto max-w-md p-4">
      <div className="rounded-2xl border border-white/10 bg-white/5 p-6 shadow-xl">
        <h1 className="mb-6 text-2xl font-semibold">Create account</h1>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
          <div>
            <label className="mb-1 block text-sm text-white/70">Full name</label>
            <input
              type="text"
              {...register("fullName")}
              className="w-full rounded-lg border border-white/10 bg-black/30 px-3 py-2 outline-none focus:border-white/30"
            />
            {errors.fullName && (
              <p className="mt-1 text-xs text-red-400">{errors.fullName.message}</p>
            )}
          </div>

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
            {isSubmitting ? "Creating…" : "Create account"}
          </button>
        </form>

        <p className="mt-4 text-sm text-white/70">
          Already have an account?{" "}
          <Link to="/login" className="text-indigo-300 hover:underline">
            Login
          </Link>
        </p>
      </div>
    </div>
  );
}
