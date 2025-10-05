using Orders.Domain.Common;

namespace Orders.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; internal set; }   // internal set so Order aggregate can assign
        public string Name { get; private set; } = default!;
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }

        public decimal LineTotal => Math.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);

        private OrderItem() { }

        public OrderItem(string name, int quantity, decimal unitPrice)
        {
            SetName(name);
            SetQuantity(quantity);
            SetUnitPrice(unitPrice);
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
            Name = name.Trim();
            Touch();
        }

        public void SetQuantity(int qty)
        {
            if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty), "Quantity must be > 0.");
            Quantity = qty;
            Touch();
        }

        public void SetUnitPrice(decimal price)
        {
            if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Unit price must be >= 0.");
            UnitPrice = Math.Round(price, 2, MidpointRounding.AwayFromZero);
            Touch();
        }
    }
}
