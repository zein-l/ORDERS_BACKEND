using System.Text.Json;
using Orders.Application.Abstractions;
using Orders.Application.DTOs;
using Orders.Application.Interfaces;   // IAuditService
using Orders.Domain.Entities;

namespace Orders.Application.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateForUserAsync(Guid userId, CancellationToken ct = default);
    Task<OrderResponse?> GetAsync(Guid currentUserId, Guid orderId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderResponse>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<OrderResponse?> AddItemAsync(Guid currentUserId, Guid orderId, AddItemRequest req, CancellationToken ct = default);
    Task<OrderResponse?> RemoveItemAsync(Guid currentUserId, Guid orderId, Guid itemId, CancellationToken ct = default);

    // NEW: status transitions (no direct property sets)
    Task<OrderResponse?> SubmitAsync(Guid currentUserId, Guid orderId, CancellationToken ct = default);
    Task<OrderResponse?> CompleteAsync(Guid currentUserId, Guid orderId, CancellationToken ct = default);
    Task<OrderResponse?> CancelAsync(Guid currentUserId, Guid orderId, CancellationToken ct = default);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;
    private readonly IUserRepository _users;
    private readonly IAuditService _audit;

    public OrderService(IOrderRepository orders, IUserRepository users, IAuditService audit)
    {
        _orders = orders;
        _users = users;
        _audit = audit;
    }

    public async Task<OrderResponse> CreateForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null) throw new InvalidOperationException("User not found.");

        var order = new Order(userId);
        await _orders.AddAsync(order, ct);
        await _orders.SaveChangesAsync(ct);

        // 🔎 Audit
        var details = JsonSerializer.Serialize(new { order.Id });
        await _audit.LogAsync(new AuditEvent(userId, "OrderCreated", order.Id, details), ct);

        return ToResponse(order);
    }

    public async Task<OrderResponse?> GetAsync(Guid currentUserId, Guid orderId, CancellationToken ct = default)
    {
        var order = await _orders.GetByIdAsync(orderId, ct);
        if (order is null || order.UserId != currentUserId) return null;
        return ToResponse(order);
    }

    public async Task<IReadOnlyList<OrderResponse>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var orders = await _orders.GetByUserAsync(userId, ct);
        return orders.Select(ToResponse).ToList();
    }

    public async Task<OrderResponse?> AddItemAsync(Guid currentUserId, Guid orderId, AddItemRequest req, CancellationToken ct = default)
    {
        var order = await _orders.GetByIdAsync(orderId, ct);
        if (order is null || order.UserId != currentUserId) return null;

        // Only editable while Draft (let the domain enforce if it already does)
        if (!string.Equals(order.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("You can only add items to a Draft order.");

        var item = new OrderItem(req.Name, req.Quantity, req.UnitPrice);
        order.AddItem(item);

        // Explicit child insert helps EF avoid wrong state updates
        await _orders.AddItemAsync(order, item, ct);
        await _orders.SaveChangesAsync(ct);

        // 🔎 Audit
        var details = JsonSerializer.Serialize(new { req.Name, req.Quantity, req.UnitPrice, NewTotal = order.Total });
        await _audit.LogAsync(new AuditEvent(currentUserId, "ItemAdded", orderId, details), ct);

        return ToResponse(order);
    }

    public async Task<OrderResponse?> RemoveItemAsync(Guid currentUserId, Guid orderId, Guid itemId, CancellationToken ct = default)
    {
        var order = await _orders.GetByIdAsync(orderId, ct);
        if (order is null || order.UserId != currentUserId) return null;

        if (!string.Equals(order.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("You can only remove items from a Draft order.");

        order.RemoveItem(itemId);
        await _orders.SaveChangesAsync(ct);

        // 🔎 Audit
        var details = JsonSerializer.Serialize(new { itemId, NewTotal = order.Total });
        await _audit.LogAsync(new AuditEvent(currentUserId, "ItemRemoved", orderId, details), ct);

        return ToResponse(order);
    }

    // ----------------------------------------------------------------
    // NEW: Status transitions via domain methods (no private setter use)
    // Allowed:
    // Draft -> Submitted
    // Submitted -> Completed
    // Draft/Submitted -> Cancelled
    // ----------------------------------------------------------------

    public async Task<OrderResponse?> SubmitAsync(Guid currentUserId, Guid orderId, CancellationToken ct = default)
    {
        var order = await _orders.GetByIdAsync(orderId, ct);
        if (order is null || order.UserId != currentUserId) return null;

        // Domain method should validate allowed transition and set timestamps internally
        order.Submit();

        await _orders.SaveChangesAsync(ct);

        // 🔎 Audit
        var details = JsonSerializer.Serialize(new
        {
            OldStatus = "Draft",
            NewStatus = order.Status,
            Items = order.Items.Count,
            Total = order.Total
        });
        await _audit.LogAsync(new AuditEvent(currentUserId, "OrderSubmitted", order.Id, details), ct);

        return ToResponse(order);
    }

    public async Task<OrderResponse?> CompleteAsync(Guid currentUserId, Guid orderId, CancellationToken ct = default)
    {
        var order = await _orders.GetByIdAsync(orderId, ct);
        if (order is null || order.UserId != currentUserId) return null;

        order.Complete();

        await _orders.SaveChangesAsync(ct);

        // 🔎 Audit
        var details = JsonSerializer.Serialize(new { OldStatus = "Submitted", NewStatus = order.Status });
        await _audit.LogAsync(new AuditEvent(currentUserId, "OrderCompleted", order.Id, details), ct);

        return ToResponse(order);
    }

    public async Task<OrderResponse?> CancelAsync(Guid currentUserId, Guid orderId, CancellationToken ct = default)
    {
        var order = await _orders.GetByIdAsync(orderId, ct);
        if (order is null || order.UserId != currentUserId) return null;

        order.Cancel();

        await _orders.SaveChangesAsync(ct);

        // 🔎 Audit
        var details = JsonSerializer.Serialize(new { OldStatus = (string?)null, NewStatus = order.Status });
        await _audit.LogAsync(new AuditEvent(currentUserId, "OrderCancelled", order.Id, details), ct);

        return ToResponse(order);
    }

    // ---------- helpers ----------

    private static OrderResponse ToResponse(Order o)
        => new(
            o.Id,
            o.UserId,
            o.Status,
            o.Total,
            o.CreatedAtUtc,
            o.UpdatedAtUtc,
            o.Items.Select(i => new OrderItemResponse(i.Id, i.Name, i.Quantity, i.UnitPrice, i.LineTotal)).ToList()
        );
}
