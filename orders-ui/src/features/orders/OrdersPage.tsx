import { useEffect, useMemo, useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { getMyOrders, createOrder, addItem, removeItem } from "./api";
import type { OrderResponse, AddItemRequest } from "./types";

function money(n: number) {
  return `$${n.toFixed(2)}`;
}

function SmallButton(props: React.ButtonHTMLAttributes<HTMLButtonElement>) {
  const { className = "", ...rest } = props;
  return (
    <button
      {...rest}
      className={`rounded-md bg-indigo-600/90 px-3 py-1 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50 ${className}`}
    />
  );
}

function GhostButton(props: React.ButtonHTMLAttributes<HTMLButtonElement>) {
  const { className = "", ...rest } = props;
  return (
    <button
      {...rest}
      className={`rounded-md border border-white/10 bg-white/5 px-3 py-1 text-sm text-white/80 hover:bg-white/10 ${className}`}
    />
  );
}

export default function OrdersPage() {
  const qc = useQueryClient();
  const [expanded, setExpanded] = useState<Record<string, boolean>>({});

  const ordersQ = useQuery({
    queryKey: ["orders", "me"],
    queryFn: getMyOrders,
    staleTime: 10_000,
  });

  const createMut = useMutation({
    mutationFn: createOrder,
    onSuccess: () => qc.invalidateQueries({ queryKey: ["orders", "me"] }),
  });

  const addItemMut = useMutation({
    mutationFn: (vars: { orderId: string; req: AddItemRequest }) =>
      addItem(vars.orderId, vars.req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["orders", "me"] }),
  });

  const removeItemMut = useMutation({
    mutationFn: (vars: { orderId: string; itemId: string }) =>
      removeItem(vars.orderId, vars.itemId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["orders", "me"] }),
  });

  const orders = ordersQ.data ?? [];

  return (
    <div className="mx-auto max-w-5xl px-6 py-10">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-3xl font-semibold">My Orders</h1>
        <SmallButton onClick={() => createMut.mutate()} disabled={createMut.isPending}>
          {createMut.isPending ? "Creating…" : "Create Order"}
        </SmallButton>
      </div>

      <div className="space-y-4">
        {orders.map((o) => (
          <OrderCard
            key={o.id}
            order={o}
            expanded={!!expanded[o.id]}
            onToggle={() => setExpanded((m) => ({ ...m, [o.id]: !m[o.id] }))}
            onAdd={(req) => addItemMut.mutate({ orderId: o.id, req })}
            onRemove={(itemId) => removeItemMut.mutate({ orderId: o.id, itemId })}
          />
        ))}
      </div>
    </div>
  );
}

function OrderCard({
  order,
  expanded,
  onToggle,
  onAdd,
  onRemove,
}: {
  order: OrderResponse;
  expanded: boolean;
  onToggle: () => void;
  onAdd: (req: AddItemRequest) => void;
  onRemove: (itemId: string) => void;
}) {
  // live computed total so the header is correct immediately
  const computedTotal = useMemo(
    () => order.items.reduce((sum, it) => sum + it.quantity * Number(it.unitPrice), 0),
    [order.items]
  );
  const displayTotal =
    order.items.length > 0 ? computedTotal : Number(order.total ?? 0);

  const [name, setName] = useState("");
  const [qty, setQty] = useState(1);
  const [price, setPrice] = useState(0.01);

  useEffect(() => {
    if (!expanded) {
      setName("");
      setQty(1);
      setPrice(0.01);
    }
  }, [expanded]);

  return (
    <div className="rounded-2xl border border-white/10 bg-white/5 shadow-xl">
      <div className="flex items-center justify-between px-5 py-4">
        <div className="space-y-1">
          <div className="text-lg font-semibold">
            <span className="text-indigo-300">#{order.id.slice(0, 8)}</span>{" "}
            <span className="text-white/80">• {order.status}</span>
          </div>
          <div className="text-xs text-white/50">
            Created: {new Date(order.createdAtUtc).toLocaleString()}
          </div>
        </div>

        <div className="flex items-center gap-3">
          <div className="text-right text-base font-semibold">{money(displayTotal)}</div>
          {!expanded ? (
            <GhostButton onClick={onToggle}>View</GhostButton>
          ) : (
            <GhostButton onClick={onToggle}>Hide</GhostButton>
          )}
        </div>
      </div>

      {expanded && (
        <div className="border-t border-white/10 px-5 py-4">
          <div className="mb-2 grid grid-cols-12 gap-3 text-sm text-white/60">
            <div className="col-span-6">Name</div>
            <div className="col-span-2">Qty</div>
            <div className="col-span-2">Unit</div>
            <div className="col-span-2">Line</div>
          </div>

          <div className="space-y-2">
            {order.items.map((it) => {
              const line = it.quantity * Number(it.unitPrice);
              return (
                <div key={it.id} className="grid grid-cols-12 items-center gap-3">
                  <div className="col-span-6">{it.name}</div>
                  <div className="col-span-2">{it.quantity}</div>
                  <div className="col-span-2">{money(Number(it.unitPrice))}</div>
                  <div className="col-span-2 flex items-center justify-between">
                    <span>{money(line)}</span>
                    <button
                      onClick={() => onRemove(it.id)}
                      className="text-sm text-red-300 underline-offset-2 hover:underline"
                    >
                      Remove
                    </button>
                  </div>
                </div>
              );
            })}
          </div>

          <div className="mt-6 grid grid-cols-12 items-end gap-3">
            <div className="col-span-6">
              <label className="mb-1 block text-xs text-white/60">Name</label>
              <input
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full rounded-lg border border-white/10 bg-black/30 px-3 py-2 outline-none focus:border-white/30"
              />
            </div>
            <div className="col-span-2">
              <label className="mb-1 block text-xs text-white/60">Quantity</label>
              <input
                type="number"
                min={1}
                value={qty}
                onChange={(e) => setQty(Math.max(1, Number(e.target.value)))}
                className="w-full rounded-lg border border-white/10 bg-black/30 px-3 py-2 outline-none focus:border-white/30"
              />
            </div>
            <div className="col-span-2">
              <label className="mb-1 block text-xs text-white/60">Unit Price</label>
              <input
                type="number"
                step="0.01"
                min={0}
                value={price}
                onChange={(e) => setPrice(Math.max(0, Number(e.target.value)))}
                className="w-full rounded-lg border border-white/10 bg-black/30 px-3 py-2 outline-none focus:border-white/30"
              />
            </div>
            <div className="col-span-2">
              <SmallButton
                className="w-full bg-emerald-600 hover:bg-emerald-500"
                onClick={() => {
                  if (!name.trim()) return;
                  onAdd({ name: name.trim(), quantity: qty, unitPrice: price });
                  setName("");
                  setQty(1);
                  setPrice(0.01);
                }}
              >
                Add Item
              </SmallButton>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
