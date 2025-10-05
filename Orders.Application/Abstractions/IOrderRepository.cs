using Orders.Domain.Entities;

namespace Orders.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);

    // New helper: add a child item with explicit EF state
    Task AddItemAsync(Order order, OrderItem item, CancellationToken ct = default);

    Task UpdateAsync(Order order, CancellationToken ct = default);
    Task DeleteAsync(Order order, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
