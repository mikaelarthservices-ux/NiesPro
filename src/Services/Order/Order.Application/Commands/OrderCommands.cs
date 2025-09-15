using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Commands;

public sealed record CreateOrderCommand(CreateOrderDto OrderData) : IRequest<Guid>;

public sealed record ConfirmOrderCommand(Guid OrderId) : IRequest<bool>;

public sealed record UpdateOrderStatusCommand(UpdateOrderStatusDto StatusData) : IRequest<bool>;

public sealed record AddOrderItemCommand(AddOrderItemDto ItemData) : IRequest<bool>;

public sealed record UpdateOrderItemCommand(UpdateOrderItemDto ItemData) : IRequest<bool>;

public sealed record RemoveOrderItemCommand(RemoveOrderItemDto ItemData) : IRequest<bool>;

public sealed record CancelOrderCommand(Guid OrderId, string Reason) : IRequest<bool>;

public sealed record ProcessPaymentCommand(ProcessPaymentDto PaymentData) : IRequest<Guid>;