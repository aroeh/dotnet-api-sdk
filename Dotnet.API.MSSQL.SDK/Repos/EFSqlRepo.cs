using Dotnet.API.MSSQL.SDK.Constants;
using Dotnet.API.MSSQL.SDK.Entities;
using Dotnet.API.MSSQL.SDK.Interfaces;
using Dotnet.API.SDK.Models;
using Microsoft.EntityFrameworkCore;

namespace Dotnet.API.MSSQL.SDK.Repos;

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
public class EFSqlRepo<TContext, TEntity>
(
    TContext dbContext
) : IEFSqlRepo<TContext, TEntity>
    where TContext : DbContext
    where TEntity : EntityBase
{
    private readonly TContext _dbContext = dbContext;
    private DbSet<TEntity> DbSet => _dbContext.Set<TEntity>();

    public IQueryable<TEntity> QueryBase => _dbContext.Set<TEntity>();

    /// <summary>
    /// Creates a new entity in the database.
    /// </summary>
    /// <param name="entity">Entity and properties to create</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Newly created entity.</returns>
    public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        await SaveAsync(cancellationToken);
        return entity;
    }

    /// <summary>
    /// Creates a many new entities in the database.
    /// </summary>
    /// <param name="entities">Collection of entities to create</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns><see cref="TransactionResult"/> results of the Create operation</returns>
    public async Task<TransactionResult> CreateManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
        var stateChanges = await SaveAsync(cancellationToken);
        return new TransactionResult(true, true, entities.Count(), stateChanges, DatabaseConstants.Created);
    }

    public async Task<IReadOnlyList<TEntity>> QueryAsync(IQueryable<TEntity> query, CancellationToken cancellationToken)
    {
        return await query.ToListAsync(cancellationToken);
    }

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
    public async Task<PaginationResponse<TEntity>> QueryAsync(IQueryable<TEntity> query, CancellationToken cancellationToken, int page = 1, int pageSize = 25)
    {
        var totalCount = await DbSet.CountAsync(cancellationToken);
        var position = page == 1 ? 0 : (page - 1) * pageSize;

        // demonstrates Offset pagination
        var results = await query
            .OrderBy(q => q.Id)
            .Skip(position)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        PaginationMetaData metaData = new(page, results.Count, pageSize, totalCount);
        return new PaginationResponse<TEntity>(results, metaData);
    }

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
    public async Task<PaginationResponse<TEntity>> QueryAsync(IQueryable<TEntity> query, PaginationQueryParametersBO pagination, CancellationToken cancellationToken)
    {
        return await QueryAsync(query, cancellationToken, pagination.Page, pagination.PageSize);
    }

    public async Task<IReadOnlyList<TEntity>> QueryAsync(CancellationToken cancellationToken, Func<IQueryable<TEntity>, IQueryable<TEntity>>? where = null)
    {
        IQueryable<TEntity> query = QueryBase;

        if (where is not null)
        {
            query = where(query);
        }

        return await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="id">Id of the entity to retrieve</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Entity matching <paramref name="id"/> if not <see langword="null"/></returns>
    public async Task<TEntity?> GetAsync(string id, CancellationToken cancellationToken)
    {
        IQueryable<TEntity> query = QueryBase;

        query = query.Where(_ => _.Id.Equals(id));

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> GetAsync(string id, CancellationToken cancellationToken, Func<IQueryable<TEntity>, IQueryable<TEntity>>? where = null)
    {
        IQueryable<TEntity> query = QueryBase;

        query = query.Where(_ => _.Id.Equals(id));

        if (where is not null)
        {
            query = where(query);
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    /*
     * for researching the function
     * https://learn.microsoft.com/en-us/dotnet/api/system.func-1?view=net-10.0
     * https://medium.com/@anuragramteke/pass-method-as-parameter-in-c-af01b71ff365
     * https://stackoverflow.com/questions/2082615/pass-method-as-parameter-using-c-sharp
     * 
     * in Func<TEntity, bool> TEntity is the input to the function, and bool is the output of the function
     */
    public async Task<TransactionResult> UpdateAsync(string id, Func<TEntity, bool> updateFunction, CancellationToken cancellationToken)
    {
        TEntity? entity = await GetAsync(id, cancellationToken);

        if (entity is null || !updateFunction(entity))
        {
            return new TransactionResult(1, DatabaseConstants.Updated);
        }

        var stateChanges = await SaveAsync(cancellationToken);
        return new TransactionResult(1, stateChanges, DatabaseConstants.Updated);
    }

    public async Task<TransactionResult> UpdateAsync(string id, TEntity updateEntity, CancellationToken cancellationToken)
    {
        var stateChanges = await SaveAsync(cancellationToken);
        return new TransactionResult(1, stateChanges, DatabaseConstants.Updated);
    }

    public async Task<TransactionResult> UpdateManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        if (entities is null || !entities.Any())
        {
            return new TransactionResult(entities?.Count() ?? 0, DatabaseConstants.Updated);
        }

        DbSet.UpdateRange(entities);
        var stateChanges = await SaveAsync(cancellationToken);
        return new TransactionResult(entities.Count(), stateChanges, DatabaseConstants.Updated);
    }

    public async Task<TransactionResult> RemoveAsync(string id, CancellationToken cancellationToken)
    {
        TEntity? entity = await GetAsync(id, cancellationToken);
        if (entity is null)
        {
            return new TransactionResult(1, DatabaseConstants.Deleted);
        }

        DbSet.Remove(entity);
        var stateChanges = await SaveAsync(cancellationToken);
        return new TransactionResult(1, stateChanges, DatabaseConstants.Deleted);
    }

    public async Task<TransactionResult> RemoveManyAsync(string[] ids, CancellationToken cancellationToken)
    {
        List<TEntity>? entities = await DbSet.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
        if (entities is null || entities.Count == 0)
        {
            return new TransactionResult(ids.Length, DatabaseConstants.Deleted);
        }

        DbSet.RemoveRange(entities);
        var stateChanges = await SaveAsync(cancellationToken);
        return new TransactionResult(ids.Length, stateChanges, DatabaseConstants.Deleted);
    }

    public async Task<int> SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // log error and handle
            throw;
        }
    }
}
