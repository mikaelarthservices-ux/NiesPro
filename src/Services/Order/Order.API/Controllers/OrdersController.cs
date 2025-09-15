using Microsoft.AspNetCore.Mvc;
using MediatR;
using Order.Application.Commands;
using Order.Application.Queries;
using Order.Application.DTOs;
using Order.Domain.Enums;
using System.Net;

namespace Order.API.Controllers;

/// <summary>
/// Controller pour la gestion des commandes avec patterns CQRS avancés
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Créer une nouvelle commande
    /// </summary>
    /// <param name="createOrderDto">Données de la commande</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>ID de la commande créée</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.UnprocessableEntity)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderDto createOrderDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating order for customer {CustomerId}", createOrderDto.CustomerId);

            var command = new CreateOrderCommand(createOrderDto);
            var orderId = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Order {OrderId} created successfully", orderId);

            return CreatedAtAction(
                nameof(GetOrder),
                new { id = orderId },
                new { OrderId = orderId });
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Validation failed for order creation: {Errors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", createOrderDto.CustomerId);
            return StatusCode(500, "An error occurred while creating the order");
        }
    }

    /// <summary>
    /// Obtenir une commande par ID
    /// </summary>
    /// <param name="id">ID de la commande</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails de la commande</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrderByIdQuery(id);
        var order = await _mediator.Send(query, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", id);
            return NotFound($"Order with ID {id} was not found");
        }

        return Ok(order);
    }

    /// <summary>
    /// Obtenir une commande par numéro
    /// </summary>
    /// <param name="orderNumber">Numéro de commande</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails de la commande</returns>
    [HttpGet("by-number/{orderNumber}")]
    [ProducesResponseType(typeof(OrderDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetOrderByNumber(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrderByNumberQuery(orderNumber);
        var order = await _mediator.Send(query, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order with number {OrderNumber} not found", orderNumber);
            return NotFound($"Order with number {orderNumber} was not found");
        }

        return Ok(order);
    }

    /// <summary>
    /// Obtenir les commandes d'un client
    /// </summary>
    /// <param name="customerId">ID du client</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste des commandes du client</returns>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetOrdersByCustomer(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrdersByCustomerQuery(customerId);
        var orders = await _mediator.Send(query, cancellationToken);

        return Ok(orders);
    }

    /// <summary>
    /// Obtenir les commandes avec pagination
    /// </summary>
    /// <param name="page">Numéro de page</param>
    /// <param name="pageSize">Taille de page</param>
    /// <param name="status">Statut optionnel</param>
    /// <param name="customerId">ID client optionnel</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Résultats paginés</returns>
    [HttpGet]
    [ProducesResponseType(typeof(NiesPro.Contracts.Common.PaginatedResult<OrderDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] Guid? customerId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrdersPagedQuery(page, pageSize, status, customerId);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Confirmer une commande
    /// </summary>
    /// <param name="id">ID de la commande</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Résultat de l'opération</returns>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ConfirmOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new ConfirmOrderCommand(id);
            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
            {
                return NotFound($"Order {id} not found or cannot be confirmed");
            }

            _logger.LogInformation("Order {OrderId} confirmed successfully", id);
            return Ok(new { Message = "Order confirmed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot confirm order {OrderId}: {Message}", id, ex.Message);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Mettre à jour le statut d'une commande
    /// </summary>
    /// <param name="id">ID de la commande</param>
    /// <param name="updateStatusDto">Données de mise à jour</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Résultat de l'opération</returns>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusDto updateStatusDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (updateStatusDto.OrderId != id)
            {
                return BadRequest("Order ID in URL must match the one in request body");
            }

            var command = new UpdateOrderStatusCommand(updateStatusDto);
            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
            {
                return NotFound($"Order {id} not found or status cannot be updated");
            }

            _logger.LogInformation("Order {OrderId} status updated to {Status}", id, updateStatusDto.NewStatus);
            return Ok(new { Message = "Order status updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot update order {OrderId} status: {Message}", id, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }

    /// <summary>
    /// Annuler une commande
    /// </summary>
    /// <param name="id">ID de la commande</param>
    /// <param name="reason">Raison de l'annulation</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Résultat de l'opération</returns>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CancelOrder(
        Guid id,
        [FromBody] string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return BadRequest("Cancellation reason is required");
            }

            var command = new CancelOrderCommand(id, reason);
            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
            {
                return NotFound($"Order {id} not found or cannot be cancelled");
            }

            _logger.LogInformation("Order {OrderId} cancelled: {Reason}", id, reason);
            return Ok(new { Message = "Order cancelled successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot cancel order {OrderId}: {Message}", id, ex.Message);
            return BadRequest(ex.Message);
        }
    }
}