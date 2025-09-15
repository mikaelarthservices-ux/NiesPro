using MediatR;
using Order.Application.DTOs;
using Order.Application.Queries;
using Order.Domain.Repositories;
using Order.Domain.Enums;
using AutoMapper;
using NiesPro.Contracts.Common;

namespace Order.Application.Queries;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetFullOrderAsync(request.OrderId, cancellationToken);
        return order != null ? _mapper.Map<OrderDto>(order) : null;
    }
}

public sealed class GetOrderByNumberQueryHandler : IRequestHandler<GetOrderByNumberQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderByNumberQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<OrderDto?> Handle(GetOrderByNumberQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByOrderNumberAsync(request.OrderNumber, cancellationToken);
        return order != null ? _mapper.Map<OrderDto>(order) : null;
    }
}

public sealed class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, IEnumerable<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }
}

public sealed class GetOrdersByStatusQueryHandler : IRequestHandler<GetOrdersByStatusQuery, IEnumerable<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersByStatusQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByStatusAsync(request.Status, cancellationToken);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }
}

public sealed class GetOrdersPagedQueryHandler : IRequestHandler<GetOrdersPagedQuery, PaginatedResult<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersPagedQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<OrderDto>> Handle(GetOrdersPagedQuery request, CancellationToken cancellationToken)
    {
        var paginatedOrders = await _orderRepository.GetPaginatedAsync(
            request.Page,
            request.PageSize,
            request.Status,
            cancellationToken);

        var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(paginatedOrders.Items);

        return PaginatedResult<OrderDto>.Create(
            orderDtos,
            paginatedOrders.TotalCount,
            request.Page,
            request.PageSize);
    }
}