using Microsoft.EntityFrameworkCore;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Orders.Infrastructure.Data;

namespace Orders.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(AuditEvent auditEvent, CancellationToken ct = default)
    {
        // defensive null check
        if (auditEvent == null) throw new ArgumentNullException(nameof(auditEvent));

        await _db.AuditEvents.AddAsync(auditEvent, ct);
        await _db.SaveChangesAsync(ct);
    }
}
