using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Customer.Application.Commands;
using Customer.Application.Queries;
using Customer.Domain.Aggregates.CustomerAggregate;

namespace Customer.API.Controllers
{
    /// <summary>
    /// Customer Address API Controller
    /// </summary>
    [ApiController]
    [Route("api/customers/{customerId:guid}/addresses")]
    [Authorize]
    public class CustomerAddressesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerAddressesController> _logger;

        public CustomerAddressesController(IMediator mediator, ILogger<CustomerAddressesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all addresses for a customer
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<CustomerAddress>>> GetCustomerAddresses(Guid customerId, [FromQuery] bool? isActive = null)
        {
            try
            {
                var query = new GetCustomerAddressesQuery 
                { 
                    CustomerId = customerId, 
                    IsActive = isActive 
                };

                var addresses = await _mediator.Send(query);
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving addresses for customer {CustomerId}", customerId);
                return StatusCode(500, "An error occurred while retrieving customer addresses");
            }
        }

        /// <summary>
        /// Get specific address by ID
        /// </summary>
        [HttpGet("{addressId:guid}")]
        public async Task<ActionResult<CustomerAddress>> GetCustomerAddress(Guid customerId, Guid addressId)
        {
            try
            {
                var query = new GetCustomerAddressByIdQuery { AddressId = addressId };
                var address = await _mediator.Send(query);

                if (address == null)
                    return NotFound($"Address with ID {addressId} not found");

                if (address.CustomerId != customerId)
                    return BadRequest("Address does not belong to the specified customer");

                return Ok(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address {AddressId} for customer {CustomerId}", addressId, customerId);
                return StatusCode(500, "An error occurred while retrieving the address");
            }
        }

        /// <summary>
        /// Add new address for customer
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> AddCustomerAddress(Guid customerId, [FromBody] AddCustomerAddressCommand command)
        {
            try
            {
                if (customerId != command.CustomerId)
                    return BadRequest("Customer ID in URL does not match command");

                var addressId = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetCustomerAddress), 
                    new { customerId, addressId }, addressId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding address for customer {CustomerId}", customerId);
                return StatusCode(500, "An error occurred while adding the address");
            }
        }

        /// <summary>
        /// Update customer address
        /// </summary>
        [HttpPut("{addressId:guid}")]
        public async Task<ActionResult> UpdateCustomerAddress(Guid customerId, Guid addressId, [FromBody] UpdateCustomerAddressCommand command)
        {
            try
            {
                if (addressId != command.AddressId)
                    return BadRequest("Address ID in URL does not match command");

                // Verify the address belongs to the customer
                var existingQuery = new GetCustomerAddressByIdQuery { AddressId = addressId };
                var existingAddress = await _mediator.Send(existingQuery);

                if (existingAddress == null)
                    return NotFound($"Address with ID {addressId} not found");

                if (existingAddress.CustomerId != customerId)
                    return BadRequest("Address does not belong to the specified customer");

                var success = await _mediator.Send(command);
                
                if (!success)
                    return NotFound($"Address with ID {addressId} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId} for customer {CustomerId}", addressId, customerId);
                return StatusCode(500, "An error occurred while updating the address");
            }
        }

        /// <summary>
        /// Delete customer address
        /// </summary>
        [HttpDelete("{addressId:guid}")]
        public async Task<ActionResult> DeleteCustomerAddress(Guid customerId, Guid addressId)
        {
            try
            {
                // Verify the address belongs to the customer
                var existingQuery = new GetCustomerAddressByIdQuery { AddressId = addressId };
                var existingAddress = await _mediator.Send(existingQuery);

                if (existingAddress == null)
                    return NotFound($"Address with ID {addressId} not found");

                if (existingAddress.CustomerId != customerId)
                    return BadRequest("Address does not belong to the specified customer");

                var command = new DeleteCustomerAddressCommand { AddressId = addressId };
                var success = await _mediator.Send(command);
                
                if (!success)
                    return NotFound($"Address with ID {addressId} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId} for customer {CustomerId}", addressId, customerId);
                return StatusCode(500, "An error occurred while deleting the address");
            }
        }
    }

    /// <summary>
    /// Customer Preferences API Controller
    /// </summary>
    [ApiController]
    [Route("api/customers/{customerId:guid}/preferences")]
    [Authorize]
    public class CustomerPreferencesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerPreferencesController> _logger;

        public CustomerPreferencesController(IMediator mediator, ILogger<CustomerPreferencesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all preferences for a customer
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<CustomerPreference>>> GetCustomerPreferences(
            Guid customerId, 
            [FromQuery] string? category = null)
        {
            try
            {
                var query = new GetCustomerPreferencesQuery 
                { 
                    CustomerId = customerId, 
                    Category = category 
                };

                var preferences = await _mediator.Send(query);
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving preferences for customer {CustomerId}", customerId);
                return StatusCode(500, "An error occurred while retrieving customer preferences");
            }
        }
    }
}