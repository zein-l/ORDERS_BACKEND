using Microsoft.EntityFrameworkCore;
using Orders.Application.Abstractions;
using Orders.Domain.Entities;
using Orders.Infrastructure.Data;

namespace Orders.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await _db.Orders.AddAsync(order, ct);
    }

    public Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _db.Attach(order);
        _db.Entry(order).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Order order, CancellationToken ct = default)
    {
        _db.Orders.Remove(order);
        return Task.CompletedTask;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<IReadOnlyList<Order>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        // ✅ Load items so the UI can render the lines
        return await _db.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task AddItemAsync(Order order, OrderItem item, CancellationToken ct = default)
    {
        if (_db.Entry(order).State == EntityState.Detached)
        {
            _db.Orders.Attach(order);
        }

        await _db.OrderItems.AddAsync(item, ct);

        // Persist computed fields from domain methods
        _db.Entry(order).Property(o => o.Total).IsModified = true;
        _db.Entry(order).Property(o => o.UpdatedAtUtc).IsModified = true;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
