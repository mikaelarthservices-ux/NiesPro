using MediatR;
using Customer.Application.Commands;
using Customer.Application.Interfaces;
using Customer.Domain.Aggregates.CustomerAggregate;
using Customer.Domain.Events;
using NiesPro.Contracts.Common;
using NiesPro.Contracts.Primitives;

namespace Customer.Application.Handlers
{
    /// <summary>
    /// Handler for AddCustomerAddressCommand
    /// </summary>
    public class AddCustomerAddressCommandHandler : IRequestHandler<AddCustomerAddressCommand, Guid>
    {
        private readonly IRepository<CustomerAddress> _addressRepository;
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;
        private readonly IEventBus _eventBus;

        public AddCustomerAddressCommandHandler(
            IRepository<CustomerAddress> addressRepository,
            IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository,
            IEventBus eventBus)
        {
            _addressRepository = addressRepository;
            _customerRepository = customerRepository;
            _eventBus = eventBus;
        }

        public async Task<Guid> Handle(AddCustomerAddressCommand request, CancellationToken cancellationToken)
        {
            // Verify customer exists
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                throw new ArgumentException($"Customer with ID {request.CustomerId} not found");

            // If this is marked as default, ensure no other addresses are default
            if (request.IsDefault)
            {
                var existingAddresses = await _addressRepository.GetAllAsync();
                var customerAddresses = existingAddresses.Where(a => a.CustomerId == request.CustomerId && a.IsActive).ToList();
                
                foreach (var addr in customerAddresses)
                {
                    if (addr.IsDefault)
                    {
                        addr.IsDefault = false;
                        await _addressRepository.UpdateAsync(addr);
                    }
                }
            }

            var address = new CustomerAddress
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                Type = request.Type,
                Label = request.Label,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                IsDefault = request.IsDefault,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _addressRepository.AddAsync(address);

            // Publish domain event
            var addressAddedEvent = new CustomerAddressAddedEvent
            {
                CustomerId = address.CustomerId,
                AddressId = address.Id,
                Type = address.Type,
                AddressLine1 = address.AddressLine1,
                City = address.City,
                Country = address.Country,
                IsDefault = address.IsDefault,
                OccurredOn = DateTime.UtcNow
            };

            await _eventBus.PublishAsync(addressAddedEvent);

            return address.Id;
        }
    }

    /// <summary>
    /// Handler for UpdateCustomerAddressCommand
    /// </summary>
    public class UpdateCustomerAddressCommandHandler : IRequestHandler<UpdateCustomerAddressCommand, bool>
    {
        private readonly IRepository<CustomerAddress> _addressRepository;
        private readonly IEventBus _eventBus;

        public UpdateCustomerAddressCommandHandler(
            IRepository<CustomerAddress> addressRepository,
            IEventBus eventBus)
        {
            _addressRepository = addressRepository;
            _eventBus = eventBus;
        }

        public async Task<bool> Handle(UpdateCustomerAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetByIdAsync(request.AddressId);
            if (address == null)
                return false;

            // If this is marked as default, ensure no other addresses are default
            if (request.IsDefault && !address.IsDefault)
            {
                var existingAddresses = await _addressRepository.GetAllAsync();
                var customerAddresses = existingAddresses.Where(a => a.CustomerId == address.CustomerId && a.IsActive && a.Id != address.Id).ToList();
                
                foreach (var addr in customerAddresses)
                {
                    if (addr.IsDefault)
                    {
                        addr.IsDefault = false;
                        await _addressRepository.UpdateAsync(addr);
                    }
                }
            }

            address.Type = request.Type;
            address.Label = request.Label;
            address.AddressLine1 = request.AddressLine1;
            address.AddressLine2 = request.AddressLine2;
            address.City = request.City;
            address.State = request.State;
            address.PostalCode = request.PostalCode;
            address.Country = request.Country;
            address.IsDefault = request.IsDefault;
            address.UpdatedAt = DateTime.UtcNow;

            await _addressRepository.UpdateAsync(address);

            return true;
        }
    }

    /// <summary>
    /// Handler for DeleteCustomerAddressCommand
    /// </summary>
    public class DeleteCustomerAddressCommandHandler : IRequestHandler<DeleteCustomerAddressCommand, bool>
    {
        private readonly IRepository<CustomerAddress> _addressRepository;

        public DeleteCustomerAddressCommandHandler(IRepository<CustomerAddress> addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<bool> Handle(DeleteCustomerAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetByIdAsync(request.AddressId);
            if (address == null)
                return false;

            // Soft delete by setting IsActive to false
            address.IsActive = false;
            address.UpdatedAt = DateTime.UtcNow;

            await _addressRepository.UpdateAsync(address);

            return true;
        }
    }

    /// <summary>
    /// Handler for DeactivateCustomerCommand
    /// </summary>
    public class DeactivateCustomerCommandHandler : IRequestHandler<DeactivateCustomerCommand, bool>
    {
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;
        private readonly IEventBus _eventBus;

        public DeactivateCustomerCommandHandler(
            IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository,
            IEventBus eventBus)
        {
            _customerRepository = customerRepository;
            _eventBus = eventBus;
        }

        public async Task<bool> Handle(DeactivateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return false;

            customer.Deactivate();
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);

            var customerDeactivatedEvent = new CustomerDeactivatedEvent
            {
                CustomerId = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Reason = request.Reason,
                OccurredOn = DateTime.UtcNow
            };

            await _eventBus.PublishAsync(customerDeactivatedEvent);

            return true;
        }
    }

    /// <summary>
    /// Handler for ActivateCustomerCommand
    /// </summary>
    public class ActivateCustomerCommandHandler : IRequestHandler<ActivateCustomerCommand, bool>
    {
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;

        public ActivateCustomerCommandHandler(IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<bool> Handle(ActivateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return false;

            customer.Activate();
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);

            return true;
        }
    }
}