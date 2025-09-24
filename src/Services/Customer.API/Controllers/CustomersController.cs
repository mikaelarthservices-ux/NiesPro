using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Customer.Application.Commands;
using Customer.Application.Queries;
using Customer.Domain.Aggregates.CustomerAggregate;

namespace Customer.API.Controllers
{
    /// <summary>
    /// Customer API Controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(IMediator mediator, ILogger<CustomersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all customers with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<Customer.Domain.Aggregates.CustomerAggregate.Customer>>> GetCustomers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? isVip = null,
            [FromQuery] string? customerType = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false)
        {
            try
            {
                var query = new GetCustomersQuery
                {
                    Page = page,
                    PageSize = Math.Min(pageSize, 100), // Limit max page size
                    SearchTerm = searchTerm,
                    IsActive = isActive,
                    IsVip = isVip,
                    CustomerType = customerType,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                return StatusCode(500, "An error occurred while retrieving customers");
            }
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Customer.Domain.Aggregates.CustomerAggregate.Customer>> GetCustomer(Guid id)
        {
            try
            {
                var query = new GetCustomerByIdQuery { CustomerId = id };
                var customer = await _mediator.Send(query);

                if (customer == null)
                    return NotFound($"Customer with ID {id} not found");

                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer {CustomerId}", id);
                return StatusCode(500, "An error occurred while retrieving the customer");
            }
        }

        /// <summary>
        /// Get customer by email
        /// </summary>
        [HttpGet("by-email/{email}")]
        public async Task<ActionResult<Customer.Domain.Aggregates.CustomerAggregate.Customer>> GetCustomerByEmail(string email)
        {
            try
            {
                var query = new GetCustomerByEmailQuery { Email = email };
                var customer = await _mediator.Send(query);

                if (customer == null)
                    return NotFound($"Customer with email {email} not found");

                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer by email {Email}", email);
                return StatusCode(500, "An error occurred while retrieving the customer");
            }
        }

        /// <summary>
        /// Search customers
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<Customer.Domain.Aggregates.CustomerAggregate.Customer>>> SearchCustomers(
            [FromQuery] string searchTerm,
            [FromQuery] int maxResults = 50)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                    return BadRequest("Search term is required");

                var query = new SearchCustomersQuery 
                { 
                    SearchTerm = searchTerm,
                    MaxResults = Math.Min(maxResults, 100)
                };

                var customers = await _mediator.Send(query);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with term {SearchTerm}", searchTerm);
                return StatusCode(500, "An error occurred while searching customers");
            }
        }

        /// <summary>
        /// Get VIP customers
        /// </summary>
        [HttpGet("vip")]
        public async Task<ActionResult<List<Customer.Domain.Aggregates.CustomerAggregate.Customer>>> GetVipCustomers([FromQuery] int? topCount = null)
        {
            try
            {
                var query = new GetVipCustomersQuery { TopCount = topCount };
                var vipCustomers = await _mediator.Send(query);
                return Ok(vipCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving VIP customers");
                return StatusCode(500, "An error occurred while retrieving VIP customers");
            }
        }

        /// <summary>
        /// Get customer statistics
        /// </summary>
        [HttpGet("{id:guid}/stats")]
        public async Task<ActionResult<CustomerStatsDto>> GetCustomerStats(Guid id)
        {
            try
            {
                var query = new GetCustomerStatsQuery { CustomerId = id };
                var stats = await _mediator.Send(query);
                return Ok(stats);
            }
            catch (ArgumentException)
            {
                return NotFound($"Customer with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer statistics for {CustomerId}", id);
                return StatusCode(500, "An error occurred while retrieving customer statistics");
            }
        }

        /// <summary>
        /// Create a new customer
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateCustomer([FromBody] CreateCustomerCommand command)
        {
            try
            {
                var customerId = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetCustomer), new { id = customerId }, customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(500, "An error occurred while creating the customer");
            }
        }

        /// <summary>
        /// Update customer information
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerCommand command)
        {
            try
            {
                if (id != command.CustomerId)
                    return BadRequest("Customer ID in URL does not match command");

                var success = await _mediator.Send(command);
                
                if (!success)
                    return NotFound($"Customer with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", id);
                return StatusCode(500, "An error occurred while updating the customer");
            }
        }

        /// <summary>
        /// Add loyalty points to customer
        /// </summary>
        [HttpPost("{id:guid}/loyalty-points")]
        public async Task<ActionResult> AddLoyaltyPoints(Guid id, [FromBody] AddLoyaltyPointsCommand command)
        {
            try
            {
                if (id != command.CustomerId)
                    return BadRequest("Customer ID in URL does not match command");

                var success = await _mediator.Send(command);
                
                if (!success)
                    return NotFound($"Customer with ID {id} not found");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loyalty points for customer {CustomerId}", id);
                return StatusCode(500, "An error occurred while adding loyalty points");
            }
        }

        /// <summary>
        /// Redeem loyalty points
        /// </summary>
        [HttpPost("{id:guid}/loyalty-points/redeem")]
        public async Task<ActionResult> RedeemLoyaltyPoints(Guid id, [FromBody] RedeemLoyaltyPointsCommand command)
        {
            try
            {
                if (id != command.CustomerId)
                    return BadRequest("Customer ID in URL does not match command");

                var success = await _mediator.Send(command);
                
                if (!success)
                    return BadRequest("Insufficient loyalty points or customer not found");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redeeming loyalty points for customer {CustomerId}", id);
                return StatusCode(500, "An error occurred while redeeming loyalty points");
            }
        }

        /// <summary>
        /// Promote customer to VIP
        /// </summary>
        [HttpPost("{id:guid}/promote-vip")]
        public async Task<ActionResult> PromoteToVip(Guid id)
        {
            try
            {
                var command = new PromoteToVipCommand { CustomerId = id };
                var success = await _mediator.Send(command);
                
                if (!success)
                    return NotFound($"Customer with ID {id} not found");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting customer {CustomerId} to VIP", id);
                return StatusCode(500, "An error occurred while promoting customer to VIP");
            }
        }

        /// <summary>
        /// Record customer order
        /// </summary>
        [HttpPost("{id:guid}/orders")]
        public async Task<ActionResult> RecordOrder(Guid id, [FromBody] RecordCustomerOrderCommand command)
        {
            try
            {
                if (id != command.CustomerId)
                    return BadRequest("Customer ID in URL does not match command");

                var success = await _mediator.Send(command);
                
                if (!success)
                    return NotFound($"Customer with ID {id} not found");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording order for customer {CustomerId}", id);
                return StatusCode(500, "An error occurred while recording the order");
            }
        }

        /// <summary>
        /// Deactivate customer
        /// </summary>
        [HttpPost("{id:guid}/deactivate")]
        public async Task<ActionResult> DeactivateCustomer(Guid id, [FromBody] DeactivateCustomerCommand command)
        {
            try
            {
                if (id != command.CustomerId)
                    return BadRequest("Customer ID in URL does not match command");

                var success = await _mediator.Send(command);
                
                if (!success)
                    return NotFound($"Customer with ID {id} not found");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating customer {CustomerId}", id);
                return StatusCode(500, "An error occurred while deactivating the customer");
            }
        }

        /// <summary>
        /// Activate customer
        /// </summary>
        [HttpPost("{id:guid}/activate")]
        public async Task<ActionResult> ActivateCustomer(Guid id)
        {
            try
            {
                var command = new ActivateCustomerCommand { CustomerId = id };
                var success = await _mediator.Send(command);
                
                if (!success)
                    return NotFound($"Customer with ID {id} not found");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating customer {CustomerId}", id);
                return StatusCode(500, "An error occurred while activating the customer");
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public ActionResult HealthCheck()
        {
            return Ok(new { Status = "Healthy", Service = "Customer.API", Timestamp = DateTime.UtcNow });
        }
    }
}