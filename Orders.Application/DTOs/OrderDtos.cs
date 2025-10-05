namespace Orders.Application.DTOs;

// Requests
public record CreateOrderRequest(Guid UserId);
public record AddItemRequest(string Name, int Quantity, decimal UnitPrice);

// Responses
public record OrderItemResponse(Guid Id, string Name, int Quantity, decimal UnitPrice, decimal LineTotal);
public record OrderResponse(
    Guid Id,
    Guid UserId,
    string Status,
    decimal Total,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    List<OrderItemResponse> Items
);
