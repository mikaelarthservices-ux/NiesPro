using MediatR;
using Customer.Application.Commands;
using Customer.Application.Queries;
using Customer.Application.Interfaces;
using Customer.Domain.Aggregates.CustomerAggregate;
using Customer.Domain.Events;
using NiesPro.Contracts.Common;
using NiesPro.Contracts.Primitives;

namespace Customer.Application.Handlers
{
    /// <summary>
    /// Handler for CreateCustomerCommand
    /// </summary>
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
    {
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;
        private readonly IEventBus _eventBus;

        public CreateCustomerCommandHandler(
            IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository,
            IEventBus eventBus)
        {
            _customerRepository = customerRepository;
            _eventBus = eventBus;
        }

        public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = new Customer.Domain.Aggregates.CustomerAggregate.Customer
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                MobilePhone = request.MobilePhone,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Notes = request.Notes,
                PreferredLanguage = request.PreferredLanguage,
                CustomerType = request.CustomerType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _customerRepository.AddAsync(customer);

            // Publish domain event
            var customerCreatedEvent = new CustomerCreatedEvent
            {
                CustomerId = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                CustomerType = customer.CustomerType,
                OccurredOn = DateTime.UtcNow
            };

            await _eventBus.PublishAsync(customerCreatedEvent);

            return customer.Id;
        }
    }

    /// <summary>
    /// Handler for UpdateCustomerCommand
    /// </summary>
    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, bool>
    {
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;
        private readonly IEventBus _eventBus;

        public UpdateCustomerCommandHandler(
            IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository,
            IEventBus eventBus)
        {
            _customerRepository = customerRepository;
            _eventBus = eventBus;
        }

        public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return false;

            var changedFields = new Dictionary<string, object>();

            if (customer.FirstName != request.FirstName)
            {
                changedFields["FirstName"] = new { Old = customer.FirstName, New = request.FirstName };
                customer.FirstName = request.FirstName;
            }

            if (customer.LastName != request.LastName)
            {
                changedFields["LastName"] = new { Old = customer.LastName, New = request.LastName };
                customer.LastName = request.LastName;
            }

            if (customer.Email != request.Email)
            {
                changedFields["Email"] = new { Old = customer.Email, New = request.Email };
                customer.Email = request.Email;
            }

            if (customer.Phone != request.Phone)
            {
                changedFields["Phone"] = new { Old = customer.Phone, New = request.Phone };
                customer.Phone = request.Phone;
            }

            if (customer.MobilePhone != request.MobilePhone)
            {
                changedFields["MobilePhone"] = new { Old = customer.MobilePhone, New = request.MobilePhone };
                customer.MobilePhone = request.MobilePhone;
            }

            if (customer.DateOfBirth != request.DateOfBirth)
            {
                changedFields["DateOfBirth"] = new { Old = customer.DateOfBirth, New = request.DateOfBirth };
                customer.DateOfBirth = request.DateOfBirth;
            }

            if (customer.Gender != request.Gender)
            {
                changedFields["Gender"] = new { Old = customer.Gender, New = request.Gender };
                customer.Gender = request.Gender;
            }

            if (customer.Notes != request.Notes)
            {
                changedFields["Notes"] = new { Old = customer.Notes, New = request.Notes };
                customer.Notes = request.Notes;
            }

            if (customer.PreferredLanguage != request.PreferredLanguage)
            {
                changedFields["PreferredLanguage"] = new { Old = customer.PreferredLanguage, New = request.PreferredLanguage };
                customer.PreferredLanguage = request.PreferredLanguage;
            }

            if (customer.CustomerType != request.CustomerType)
            {
                changedFields["CustomerType"] = new { Old = customer.CustomerType, New = request.CustomerType };
                customer.CustomerType = request.CustomerType;
            }

            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);

            if (changedFields.Any())
            {
                var customerUpdatedEvent = new CustomerUpdatedEvent
                {
                    CustomerId = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    ChangedFields = changedFields,
                    OccurredOn = DateTime.UtcNow
                };

                await _eventBus.PublishAsync(customerUpdatedEvent);
            }

            return true;
        }
    }

    /// <summary>
    /// Handler for AddLoyaltyPointsCommand
    /// </summary>
    public class AddLoyaltyPointsCommandHandler : IRequestHandler<AddLoyaltyPointsCommand, bool>
    {
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;
        private readonly IEventBus _eventBus;

        public AddLoyaltyPointsCommandHandler(
            IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository,
            IEventBus eventBus)
        {
            _customerRepository = customerRepository;
            _eventBus = eventBus;
        }

        public async Task<bool> Handle(AddLoyaltyPointsCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return false;

            customer.AddLoyaltyPoints(request.Points);
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);

            var loyaltyPointsAddedEvent = new LoyaltyPointsAddedEvent
            {
                CustomerId = customer.Id,
                PointsAdded = request.Points,
                TotalPoints = customer.LoyaltyPoints,
                Reason = request.Reason,
                OccurredOn = DateTime.UtcNow
            };

            await _eventBus.PublishAsync(loyaltyPointsAddedEvent);

            return true;
        }
    }

    /// <summary>
    /// Handler for RedeemLoyaltyPointsCommand
    /// </summary>
    public class RedeemLoyaltyPointsCommandHandler : IRequestHandler<RedeemLoyaltyPointsCommand, bool>
    {
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;
        private readonly IEventBus _eventBus;

        public RedeemLoyaltyPointsCommandHandler(
            IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository,
            IEventBus eventBus)
        {
            _customerRepository = customerRepository;
            _eventBus = eventBus;
        }

        public async Task<bool> Handle(RedeemLoyaltyPointsCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return false;

            try
            {
                customer.RedeemLoyaltyPoints(request.Points);
                customer.UpdatedAt = DateTime.UtcNow;
                await _customerRepository.UpdateAsync(customer);

                var loyaltyPointsRedeemedEvent = new LoyaltyPointsRedeemedEvent
                {
                    CustomerId = customer.Id,
                    PointsRedeemed = request.Points,
                    RemainingPoints = customer.LoyaltyPoints,
                    Reason = request.Reason,
                    OccurredOn = DateTime.UtcNow
                };

                await _eventBus.PublishAsync(loyaltyPointsRedeemedEvent);

                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Handler for PromoteToVipCommand
    /// </summary>
    public class PromoteToVipCommandHandler : IRequestHandler<PromoteToVipCommand, bool>
    {
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;
        private readonly IEventBus _eventBus;

        public PromoteToVipCommandHandler(
            IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository,
            IEventBus eventBus)
        {
            _customerRepository = customerRepository;
            _eventBus = eventBus;
        }

        public async Task<bool> Handle(PromoteToVipCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return false;

            customer.PromoteToVip();
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);

            var customerPromotedToVipEvent = new CustomerPromotedToVipEvent
            {
                CustomerId = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                TotalSpent = customer.TotalSpent,
                TotalOrders = customer.TotalOrders,
                OccurredOn = DateTime.UtcNow
            };

            await _eventBus.PublishAsync(customerPromotedToVipEvent);

            return true;
        }
    }

    /// <summary>
    /// Handler for RecordCustomerOrderCommand
    /// </summary>
    public class RecordCustomerOrderCommandHandler : IRequestHandler<RecordCustomerOrderCommand, bool>
    {
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;
        private readonly IEventBus _eventBus;

        public RecordCustomerOrderCommandHandler(
            IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository,
            IEventBus eventBus)
        {
            _customerRepository = customerRepository;
            _eventBus = eventBus;
        }

        public async Task<bool> Handle(RecordCustomerOrderCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return false;

            customer.RecordOrder(request.OrderAmount);
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);

            var customerOrderRecordedEvent = new CustomerOrderRecordedEvent
            {
                CustomerId = customer.Id,
                OrderId = request.OrderId,
                OrderAmount = request.OrderAmount,
                TotalOrders = customer.TotalOrders,
                TotalSpent = customer.TotalSpent,
                OrderDate = request.OrderDate,
                OccurredOn = DateTime.UtcNow
            };

            await _eventBus.PublishAsync(customerOrderRecordedEvent);

            return true;
        }
    }
}