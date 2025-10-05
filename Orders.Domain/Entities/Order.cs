using Orders.Domain.Common;

namespace Orders.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; private set; }
        public decimal Total { get; private set; }
        public string Status { get; private set; } = "Draft";
        public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

        private Order() { }

        public Order(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("UserId required.", nameof(userId));
            UserId = userId;
            Total = 0m;
            Status = "Draft";
        }

        // -----------------------
        // Item management
        // -----------------------

        public void AddItem(OrderItem item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            EnsureDraft();

            item.OrderId = Id; // FK set here (same assembly)
            Items.Add(item);
            RecalculateTotal();
        }

        public void RemoveItem(Guid itemId)
        {
            EnsureDraft();

            var item = Items.FirstOrDefault(i => i.Id == itemId);
            if (item is null) return;
            Items.Remove(item);
            RecalculateTotal();
        }

        public void RecalculateTotal()
        {
            Total = Math.Round(Items.Sum(i => i.LineTotal), 2, MidpointRounding.AwayFromZero);
            Touch();
        }

        // -----------------------
        // Status transitions
        // -----------------------

        public void Submit()
        {
            if (Status != "Draft")
                throw new InvalidOperationException("Only draft orders can be submitted.");
            Status = "Submitted";
            Touch();
        }

        public void Complete()
        {
            if (Status != "Submitted")
                throw new InvalidOperationException("Only submitted orders can be completed.");
            Status = "Completed";
            Touch();
        }

        public void Cancel()
        {
            if (Status == "Completed")
                throw new InvalidOperationException("Completed orders cannot be cancelled.");
            Status = "Cancelled";
            Touch();
        }

        // -----------------------
        // Utility
        // -----------------------

        public void SetStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) throw new ArgumentException("Status required.", nameof(status));
            Status = status.Trim();
            Touch();
        }

        private void EnsureDraft()
        {
            if (!string.Equals(Status, "Draft", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only Draft orders can be modified.");
        }
    }
}
