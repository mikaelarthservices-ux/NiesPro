using Microsoft.Extensions.Logging;
using MediatR;

namespace NiesPro.Contracts.Application.CQRS
{
    /// <summary>
    /// Marker interface for all commands
    /// Commands represent write operations that change system state
    /// Migrated from BuildingBlocks.Common.CQRS
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
    /// Base command handler with logging support
    /// </summary>
    /// <typeparam name="TCommand">Command type</typeparam>
    public abstract class BaseCommandHandler<TCommand> where TCommand : ICommand
    {
        protected readonly ILogger Logger;

        protected BaseCommandHandler(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handle command execution with logging
        /// </summary>
        protected virtual async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Handling command {CommandType} with ID {CommandId}", 
                typeof(TCommand).Name, command.CommandId);

            try
            {
                await ExecuteAsync(command, cancellationToken);
                
                Logger.LogInformation("Successfully handled command {CommandType} with ID {CommandId}", 
                    typeof(TCommand).Name, command.CommandId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error handling command {CommandType} with ID {CommandId}", 
                    typeof(TCommand).Name, command.CommandId);
                throw;
            }
        }

        /// <summary>
        /// Execute the command logic
        /// </summary>
        protected abstract Task ExecuteAsync(TCommand command, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Base command handler with response and logging support
    /// </summary>
    /// <typeparam name="TCommand">Command type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public abstract class BaseCommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
    {
        protected readonly ILogger Logger;

        protected BaseCommandHandler(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handle command execution with logging
        /// </summary>
        protected virtual async Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Handling command {CommandType} with ID {CommandId}", 
                typeof(TCommand).Name, command.CommandId);

            try
            {
                var result = await ExecuteAsync(command, cancellationToken);
                
                Logger.LogInformation("Successfully handled command {CommandType} with ID {CommandId}", 
                    typeof(TCommand).Name, command.CommandId);
                
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error handling command {CommandType} with ID {CommandId}", 
                    typeof(TCommand).Name, command.CommandId);
                throw;
            }
        }

        /// <summary>
        /// Execute the command logic
        /// </summary>
        protected abstract Task<TResponse> ExecuteAsync(TCommand command, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Base query handler with logging support
    /// </summary>
    /// <typeparam name="TQuery">Query type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public abstract class BaseQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
    {
        protected readonly ILogger Logger;

        protected BaseQueryHandler(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handle query execution with logging
        /// </summary>
        protected virtual async Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Handling query {QueryType} with ID {QueryId}", 
                typeof(TQuery).Name, query.QueryId);

            try
            {
                var result = await ExecuteAsync(query, cancellationToken);
                
                Logger.LogInformation("Successfully handled query {QueryType} with ID {QueryId}", 
                    typeof(TQuery).Name, query.QueryId);
                
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error handling query {QueryType} with ID {QueryId}", 
                    typeof(TQuery).Name, query.QueryId);
                throw;
            }
        }

        /// <summary>
        /// Execute the query logic
        /// </summary>
        protected abstract Task<TResponse> ExecuteAsync(TQuery query, CancellationToken cancellationToken);
    }
}