using Orders.Domain.Entities;

namespace Orders.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditEvent auditEvent, CancellationToken ct = default);
}
