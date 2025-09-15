using Microsoft.AspNetCore.Mvc;
using MediatR;
using Order.Application.Commands;
using Order.Application.DTOs;
using System.Net;

namespace Order.API.Controllers;

/// <summary>
/// Controller pour la gestion des articles de commande
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class OrderItemsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderItemsController> _logger;

    public OrderItemsController(IMediator mediator, ILogger<OrderItemsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Ajouter un article à une commande
    /// </summary>
    /// <param name="addItemDto">Données de l'article</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Résultat de l'opération</returns>
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> AddOrderItem(
        [FromBody] AddOrderItemDto addItemDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new AddOrderItemCommand(addItemDto);
            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
            {
                return NotFound($"Order {addItemDto.OrderId} not found or item cannot be added");
            }

            _logger.LogInformation("Item {ProductName} added to order {OrderId}", 
                addItemDto.ProductName, addItemDto.OrderId);

            return Ok(new { Message = "Item added to order successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot add item to order {OrderId}: {Message}", addItemDto.OrderId, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }

    /// <summary>
    /// Mettre à jour la quantité d'un article
    /// </summary>
    /// <param name="updateItemDto">Données de mise à jour</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Résultat de l'opération</returns>
    [HttpPut]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateOrderItem(
        [FromBody] UpdateOrderItemDto updateItemDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new UpdateOrderItemCommand(updateItemDto);
            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
            {
                return NotFound($"Order {updateItemDto.OrderId} or product {updateItemDto.ProductId} not found");
            }

            _logger.LogInformation("Item quantity updated in order {OrderId} for product {ProductId}", 
                updateItemDto.OrderId, updateItemDto.ProductId);

            return Ok(new { Message = "Item quantity updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot update item in order {OrderId}: {Message}", updateItemDto.OrderId, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }

    /// <summary>
    /// Supprimer un article d'une commande
    /// </summary>
    /// <param name="removeItemDto">Données de suppression</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Résultat de l'opération</returns>
    [HttpDelete]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> RemoveOrderItem(
        [FromBody] RemoveOrderItemDto removeItemDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new RemoveOrderItemCommand(removeItemDto);
            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
            {
                return NotFound($"Order {removeItemDto.OrderId} or product {removeItemDto.ProductId} not found");
            }

            _logger.LogInformation("Item removed from order {OrderId} for product {ProductId}", 
                removeItemDto.OrderId, removeItemDto.ProductId);

            return Ok(new { Message = "Item removed from order successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot remove item from order {OrderId}: {Message}", removeItemDto.OrderId, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }
}

/// <summary>
/// Controller pour la gestion des paiements
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Traiter un paiement pour une commande
    /// </summary>
    /// <param name="processPaymentDto">Données du paiement</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>ID du paiement créé</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> ProcessPayment(
        [FromBody] ProcessPaymentDto processPaymentDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new ProcessPaymentCommand(processPaymentDto);
            var paymentId = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Payment {PaymentId} processed for order {OrderId}", 
                paymentId, processPaymentDto.OrderId);

            return CreatedAtAction(
                nameof(GetPayment),
                new { id = paymentId },
                new { PaymentId = paymentId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot process payment for order {OrderId}: {Message}", 
                processPaymentDto.OrderId, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }

    /// <summary>
    /// Obtenir les détails d'un paiement
    /// </summary>
    /// <param name="id">ID du paiement</param>
    /// <returns>Détails du paiement</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public IActionResult GetPayment(Guid id)
    {
        // Cette méthode serait implémentée avec une query dédiée
        return Ok(new { PaymentId = id, Status = "Placeholder for payment details" });
    }
}