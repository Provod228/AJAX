using WebApplication3.Application.Common;

namespace WebApplication3.Application.Orders.GetOrder;

public sealed record GetOrderQuery(Guid OrderId);

public sealed record OrderDto(
    Guid Id,
    string CustomerEmail,
    decimal Total,
    IReadOnlyList<OrderItemDto> Items);

public sealed record OrderItemDto(Guid ProductId, string Name, decimal Price, int Quantity);
// Application/Orders/GetOrder/GetOrderUseCase.cs
using Shop.Application.Common;
using Shop.Domain;

namespace Shop.Application.Orders.GetOrder;

public sealed class GetOrderUseCase
{
    private readonly IOrderRepository _orders;
    public GetOrderUseCase(IOrderRepository orders) => _orders = orders;

    public async Task<Result<OrderDto>> ExecuteAsync(GetOrderQuery query, CancellationToken ct)
    {
        var order = await _orders.GetAsync(query.OrderId, ct);
        if (order is null) return Error.NotFound("order.not_found", "Заказ не найден");

        return new OrderDto(
            order.Id,
            order.CustomerEmail,
            order.Total,
            order.Items.Select(i => new OrderItemDto(i.ProductId, i.Name, i.Price, i.Quantity)).ToList());
    }
}