namespace ArtGallery.Domain.Interfaces;

/// <summary>
/// Unit of Work interface for managing database transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the repository for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <returns>The repository instance.</returns>
    IRepository<T> Repository<T>() where T : class;

    /// <summary>
    /// Commits all changes to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> CommitAsync();

    /// <summary>
    /// Rolls back all uncommitted changes.
    /// </summary>
    Task RollbackAsync();
}
