namespace Payment.Domain.Interfaces;

/// <summary>
/// Interface pour le pattern Unit of Work - NiesPro Enterprise Standard
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Sauvegarde tous les changements en attente
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Nombre d'entités affectées</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Démarre une transaction de base de données
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Valide la transaction courante
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Annule la transaction courante
    /// </summary>
    Task RollbackTransactionAsync();
}