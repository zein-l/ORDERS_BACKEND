using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orders.Api.Security;            // for User.GetUserId()
using Orders.Application.DTOs;
using Orders.Application.Services;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize] // all endpoints here require a valid JWT
public class OrdersController : ControllerBase
{
    private readonly IOrderService _svc;
    private readonly IValidator<AddItemRequest> _addItemValidator;

    public OrdersController(IOrderService svc, IValidator<AddItemRequest> addItemValidator)
    {
        _svc = svc;
        _addItemValidator = addItemValidator;
    }

    /// <summary>Create a new order for the current user.</summary>
    [HttpPost("orders")]
    public async Task<ActionResult<OrderResponse>> Create(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _svc.CreateForUserAsync(userId.Value, ct);
        return CreatedAtAction(nameof(GetById), new { orderId = result.Id }, result);
    }

    /// <summary>Get an order by id (must belong to current user).</summary>
    [HttpGet("orders/{orderId:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById([FromRoute] Guid orderId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _svc.GetAsync(userId.Value, orderId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>List current user's orders (original endpoint, unchanged).</summary>
    [HttpGet("me/orders")]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetMyOrders(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _svc.GetByUserAsync(userId.Value, ct);
        return Ok(result);
    }

    /// <summary>
    /// List current user's orders with paging/sorting/filtering.
    /// Example: GET /api/me/orders/paged?page=1&pageSize=20&sort=-createdAtUtc&status=Submitted
    /// </summary>
    [HttpGet("me/orders/paged")]
    public async Task<ActionResult<PagedResult<OrderResponse>>> GetMyOrdersPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sort = "-createdAtUtc",
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        // Reuse existing service & in-memory page to avoid contract churn
        var all = await _svc.GetByUserAsync(userId.Value, ct);
        IEnumerable<OrderResponse> query = all;

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => string.Equals(o.Status, status, StringComparison.OrdinalIgnoreCase));

        var (field, desc) = ParseSort(sort);
        query = field switch
        {
            "total" => desc ? query.OrderByDescending(o => o.Total) : query.OrderBy(o => o.Total),
            "status" => desc ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
            "createdatutc" => desc ? query.OrderByDescending(o => o.CreatedAtUtc) : query.OrderBy(o => o.CreatedAtUtc),
            _ => desc ? query.OrderByDescending(o => o.CreatedAtUtc) : query.OrderBy(o => o.CreatedAtUtc)
        };

        var total = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new PagedResult<OrderResponse>(items, page, pageSize, total));

        static (string field, bool desc) ParseSort(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return ("createdatutc", true);
            s = s.Trim();
            var desc = s.StartsWith("-");
            var field = s.TrimStart('-').ToLowerInvariant();
            return (field, desc);
        }
    }

    /// <summary>Add an item to an order (must belong to current user).</summary>
    [HttpPost("orders/{orderId:guid}/items")]
    public async Task<ActionResult<OrderResponse>> AddItem(
        [FromRoute] Guid orderId,
        [FromBody] AddItemRequest req,
        CancellationToken ct)
    {
        var v = await _addItemValidator.ValidateAsync(req, ct);
        if (!v.IsValid)
        {
            var ms = new ModelStateDictionary();
            foreach (var e in v.Errors)
                ms.AddModelError(e.PropertyName, e.ErrorMessage);

            return ValidationProblem(ms);
        }

        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _svc.AddItemAsync(userId.Value, orderId, req, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Remove an item from an order (must belong to current user).</summary>
    [HttpDelete("orders/{orderId:guid}/items/{itemId:guid}")]
    public async Task<ActionResult<OrderResponse>> RemoveItem(
        [FromRoute] Guid orderId,
        [FromRoute] Guid itemId,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _svc.RemoveItemAsync(userId.Value, orderId, itemId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    // ---------- NEW: status transition endpoints ----------

    /// <summary>Submit a Draft order.</summary>
    [HttpPost("orders/{orderId:guid}/submit")]
    public async Task<ActionResult<OrderResponse>> Submit([FromRoute] Guid orderId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _svc.SubmitAsync(userId.Value, orderId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Complete a Submitted order.</summary>
    [HttpPost("orders/{orderId:guid}/complete")]
    public async Task<ActionResult<OrderResponse>> Complete([FromRoute] Guid orderId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _svc.CompleteAsync(userId.Value, orderId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Cancel a Draft or Submitted order.</summary>
    [HttpPost("orders/{orderId:guid}/cancel")]
    public async Task<ActionResult<OrderResponse>> Cancel([FromRoute] Guid orderId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _svc.CancelAsync(userId.Value, orderId, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
