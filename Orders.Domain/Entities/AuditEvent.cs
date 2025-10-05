using Orders.Domain.Common;

namespace Orders.Domain.Entities;

public class AuditEvent : BaseEntity
{
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = default!;
    public Guid? OrderId { get; private set; }
    public string? DetailsJson { get; private set; }

    private AuditEvent() { }

    public AuditEvent(Guid? userId, string action, Guid? orderId = null, string? detailsJson = null)
    {
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action required.", nameof(action));
        UserId = userId;
        Action = action.Trim();
        OrderId = orderId;
        DetailsJson = detailsJson;
    }
}
