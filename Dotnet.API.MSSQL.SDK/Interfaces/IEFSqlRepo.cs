using Dotnet.API.MSSQL.SDK.Entities;
using Dotnet.API.SDK.Models;
using Microsoft.EntityFrameworkCore;

namespace Dotnet.API.MSSQL.SDK.Interfaces;

/// <summary>
/// Entity Framework MS SQL Repository
/// </summary>
/// <remarks>
/// <para>
/// Methods for executing CRUD operations on a SQL database using entity framework
/// </para>
/// </remarks>
/// <typeparam name="TContext">Context of type <see cref="DbContext"/></typeparam>
/// <typeparam name="TEntity">Entity of type <see cref="EntityBase"/></typeparam>
public interface IEFSqlRepo<TContext, TEntity>
    where TContext : DbContext
    where TEntity : EntityBase
{
    IQueryable<TEntity> QueryBase { get; }

    /// <summary>
    /// Creates a new entity in the database.
    /// </summary>
    /// <param name="entity">Entity and properties to create</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Newly created entity.</returns>
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a many new entities in the database.
    /// </summary>
    /// <param name="entities">Collection of entities to create</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns><see cref="TransactionResult"/> results of the Create operation</returns>
    Task<TransactionResult> CreateManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);


    Task<IReadOnlyList<TEntity>> QueryAsync(IQueryable<TEntity> where, CancellationToken cancellationToken);

    /// <summary>
    /// Query entities using filters and pagination parameters
    /// </summary>
    /// <param name="query">Query to filter entities</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <param name="page">Page number to retrieve.  Defaults to 1.</param>
    /// <param name="pageSize">Number of results to return for each page.  Defaults to 25.</param>
    /// <remarks>
    /// Uses Offset pagination implementation
    /// </remarks>
    /// <returns>Paginated response for entities</returns>
    Task<PaginationResponse<TEntity>> QueryAsync(IQueryable<TEntity> query, CancellationToken cancellationToken, int page = 1, int pageSize = 25);

    /// <summary>
    /// Query entities using filters and pagination parameters
    /// </summary>
    /// <param name="query">Query to filter entities</param>
    /// <param name="pagination">Pagination query parameters</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <remarks>
    /// Uses Offset pagination implementation
    /// </remarks>
    /// <returns>Paginated response for entities</returns>
    Task<PaginationResponse<TEntity>> QueryAsync(IQueryable<TEntity> query, PaginationQueryParametersBO pagination, CancellationToken cancellationToken);

    Task<IReadOnlyList<TEntity>> QueryAsync(CancellationToken cancellationToken, Func<IQueryable<TEntity>, IQueryable<TEntity>>? where = null);

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="id">Id of the entity to retrieve</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Entity matching <paramref name="id"/> if not <see langword="null"/></returns>
    Task<TEntity?> GetAsync(string id, CancellationToken cancellationToken);

    Task<TEntity?> GetAsync(string id, CancellationToken cancellationToken, Func<IQueryable<TEntity>, IQueryable<TEntity>>? where = null);

    /// <summary>
    /// TODO: This method doesn't work - need to investigate why
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateFunction"></param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns></returns>
    Task<TransactionResult> UpdateAsync(string id, Func<TEntity, bool> updateFunction, CancellationToken cancellationToken);

    Task<TransactionResult> UpdateAsync(string id, TEntity updateEntity, CancellationToken cancellationToken);

    Task<TransactionResult> UpdateManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);

    Task<TransactionResult> RemoveAsync(string id, CancellationToken cancellationToken);

    Task<TransactionResult> RemoveManyAsync(string[] ids, CancellationToken cancellationToken);

    Task<int> SaveAsync(CancellationToken cancellationToken);
}
