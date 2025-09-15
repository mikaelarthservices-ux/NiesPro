using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Common.CQRS
{
    /// <summary>
    /// Base interface for MediatR requests (placeholder)
    /// </summary>
    public interface IRequest { }

    /// <summary>
    /// Base interface for MediatR requests with response (placeholder)
    /// </summary>
    public interface IRequest<out TResponse> { }

    /// <summary>
    /// Base interface for MediatR request handlers (placeholder)
    /// </summary>
    public interface IRequestHandler<in TRequest> where TRequest : IRequest
    {
        Task Handle(TRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Base interface for MediatR request handlers with response (placeholder)
    /// </summary>
    public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Marker interface for all commands
    /// Commands represent write operations that change system state
    /// </summary>
    public interface ICommand : IRequest
    {
        /// <summary>
        /// Unique identifier for tracking this command
        /// </summary>
        Guid CommandId { get; }

        /// <summary>
        /// Timestamp when the command was created
        /// </summary>
        DateTime Timestamp { get; }
    }

    /// <summary>
    /// Command interface with return value
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface ICommand<out TResponse> : IRequest<TResponse>
    {
        /// <summary>
        /// Unique identifier for tracking this command
        /// </summary>
        Guid CommandId { get; }

        /// <summary>
        /// Timestamp when the command was created
        /// </summary>
        DateTime Timestamp { get; }
    }

    /// <summary>
    /// Marker interface for all queries
    /// Queries represent read operations that don't change system state
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface IQuery<out TResponse> : IRequest<TResponse>
    {
        /// <summary>
        /// Unique identifier for tracking this query
        /// </summary>
        Guid QueryId { get; }

        /// <summary>
        /// Timestamp when the query was created
        /// </summary>
        DateTime Timestamp { get; }
    }

    /// <summary>
    /// Base implementation for commands
    /// </summary>
    public abstract class BaseCommand : ICommand
    {
        /// <inheritdoc />
        public Guid CommandId { get; } = Guid.NewGuid();

        /// <inheritdoc />
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Base implementation for commands with response
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    public abstract class BaseCommand<TResponse> : ICommand<TResponse>
    {
        /// <inheritdoc />
        public Guid CommandId { get; } = Guid.NewGuid();

        /// <inheritdoc />
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Base implementation for queries
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    public abstract class BaseQuery<TResponse> : IQuery<TResponse>
    {
        /// <inheritdoc />
        public Guid QueryId { get; } = Guid.NewGuid();

        /// <inheritdoc />
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Base class for command handlers providing common functionality
    /// </summary>
    /// <typeparam name="TCommand">Command type</typeparam>
    public abstract class BaseCommandHandler<TCommand> : IRequestHandler<TCommand>
        where TCommand : ICommand
    {
        /// <summary>
        /// Logger instance for this handler
        /// </summary>
        protected readonly ILogger<BaseCommandHandler<TCommand>> Logger;

        /// <summary>
        /// Initializes the base command handler
        /// </summary>
        /// <param name="logger">Logger instance</param>
        protected BaseCommandHandler(ILogger<BaseCommandHandler<TCommand>> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the command execution
        /// </summary>
        /// <param name="request">Command to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        public async Task Handle(TCommand request, CancellationToken cancellationToken)
        {
            var commandName = typeof(TCommand).Name;
            
            Logger.LogInformation(
                "Handling command {CommandName} with ID {CommandId} at {Timestamp}",
                commandName, request.CommandId, request.Timestamp);

            try
            {
                await HandleCommand(request, cancellationToken);
                
                Logger.LogInformation(
                    "Successfully handled command {CommandName} with ID {CommandId}",
                    commandName, request.CommandId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Error handling command {CommandName} with ID {CommandId}: {ErrorMessage}",
                    commandName, request.CommandId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Override this method to implement command-specific logic
        /// </summary>
        /// <param name="request">Command to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        protected abstract Task HandleCommand(TCommand request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Base class for command handlers with response providing common functionality
    /// </summary>
    /// <typeparam name="TCommand">Command type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public abstract class BaseCommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        /// <summary>
        /// Logger instance for this handler
        /// </summary>
        protected readonly ILogger<BaseCommandHandler<TCommand, TResponse>> Logger;

        /// <summary>
        /// Initializes the base command handler
        /// </summary>
        /// <param name="logger">Logger instance</param>
        protected BaseCommandHandler(ILogger<BaseCommandHandler<TCommand, TResponse>> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the command execution with response
        /// </summary>
        /// <param name="request">Command to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Command response</returns>
        public async Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken)
        {
            var commandName = typeof(TCommand).Name;
            
            Logger.LogInformation(
                "Handling command {CommandName} with ID {CommandId} at {Timestamp}",
                commandName, request.CommandId, request.Timestamp);

            try
            {
                var response = await HandleCommand(request, cancellationToken);
                
                Logger.LogInformation(
                    "Successfully handled command {CommandName} with ID {CommandId}",
                    commandName, request.CommandId);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Error handling command {CommandName} with ID {CommandId}: {ErrorMessage}",
                    commandName, request.CommandId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Override this method to implement command-specific logic
        /// </summary>
        /// <param name="request">Command to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Command response</returns>
        protected abstract Task<TResponse> HandleCommand(TCommand request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Base class for query handlers providing common functionality
    /// </summary>
    /// <typeparam name="TQuery">Query type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public abstract class BaseQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        /// <summary>
        /// Logger instance for this handler
        /// </summary>
        protected readonly ILogger<BaseQueryHandler<TQuery, TResponse>> Logger;

        /// <summary>
        /// Initializes the base query handler
        /// </summary>
        /// <param name="logger">Logger instance</param>
        protected BaseQueryHandler(ILogger<BaseQueryHandler<TQuery, TResponse>> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the query execution
        /// </summary>
        /// <param name="request">Query to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Query response</returns>
        public async Task<TResponse> Handle(TQuery request, CancellationToken cancellationToken)
        {
            var queryName = typeof(TQuery).Name;
            
            Logger.LogInformation(
                "Handling query {QueryName} with ID {QueryId} at {Timestamp}",
                queryName, request.QueryId, request.Timestamp);

            try
            {
                var response = await HandleQuery(request, cancellationToken);
                
                Logger.LogInformation(
                    "Successfully handled query {QueryName} with ID {QueryId}",
                    queryName, request.QueryId);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Error handling query {QueryName} with ID {QueryId}: {ErrorMessage}",
                    queryName, request.QueryId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Override this method to implement query-specific logic
        /// </summary>
        /// <param name="request">Query to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Query response</returns>
        protected abstract Task<TResponse> HandleQuery(TQuery request, CancellationToken cancellationToken);
    }
}