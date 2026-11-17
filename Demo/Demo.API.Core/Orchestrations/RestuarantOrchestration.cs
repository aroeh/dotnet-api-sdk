using Demo.API.Core.Extensions;
using Demo.API.Core.Interfaces;
using Demo.API.Infrastructure.MongoDb.Interfaces;
using Demo.API.Shared.Models;
using Dotnet.API.Redis.SDK.Interfaces;
using Dotnet.API.SDK.Exceptions;
using Dotnet.API.SDK.Models;
using Dotnet.API.SDK.Utils;
using Microsoft.Extensions.Logging;

namespace Demo.API.Core.Orchestrations;

public class RestuarantOrchestration(ILogger<RestuarantOrchestration> logger, IRestuarantRepo repo, IRedisRepo redis) : IRestuarantOrchestration
{
    private readonly ILogger<RestuarantOrchestration> _logger = logger;
    private readonly IRestuarantRepo _repo = repo;
    private readonly IRedisRepo _redis = redis;

    /// <summary>
    /// List restuarants
    /// </summary>
    /// <param name="queryParameters">Optional - Query parameters to filter restuarants</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Paginated list of restuarants matching <paramref name="queryParameters"/></returns>
    public async Task<PaginationResponse<RestuarantBO>> ListRestuarantsAsync(RestuarantFilterQueryParametersBO queryParameters, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Querying restuarants");
        return await _repo.QueryRestuarantsAsync(queryParameters, cancellationToken);
    }

    /// <summary>
    /// Get restuarant by id
    /// </summary>
    /// <param name="id">Id of the restuarant</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Restuarant if not <see langword="null"/></returns>
    public async Task<RestuarantBO?> GetRestuarantAsync(string id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting restuarant by id");

        RestuarantBO? cache = await _redis.GetCacheValueAsync<RestuarantBO>(id, cancellationToken);
        if (cache is not null)
        {
            return cache;
        }

        RestuarantBO? restuarant = await _repo.GetRestuarantAsync(id, cancellationToken);
        if (restuarant is not null)
        {
            await _redis.AddCacheValueAsync(id, restuarant, cancellationToken);
        }

        return restuarant;
    }

    /// <summary>
    /// Creates a new Restuarant
    /// </summary>
    /// <param name="request">Restuarant properties and data</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Restuarant object updated with the new id</returns>
    public async Task<RestuarantBO> CreateRestuarantAsync(CreateRestuarantRequestBO request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding new restuarant");
        string newId = IdGenerator.GenerateId();
        RestuarantBO restuarant = request.MapToRestuarant(newId);

        if (request.BusinessHours?.Count > 0)
        {
            foreach (var item in request.BusinessHours)
            {
                string newHourId = IdGenerator.GenerateId();
                restuarant.BusinessHours.Add(item.MapToBusinessHours(newHourId));
            }
        }

        RestuarantBO newRestuarant = await _repo.CreateRestuarantAsync(restuarant, cancellationToken);
        await _redis.AddCacheValueAsync(newRestuarant.Id, newRestuarant, TimeSpan.FromMinutes(5), cancellationToken);
        return newRestuarant;
    }

    /// <summary>
    /// Create many new Restuarants
    /// </summary>
    /// <param name="requests">Collection of create restuarant requests</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Create results for the transaction</returns>
    public async Task<TransactionResult> CreateManyRestuarantsAsync(CreateRestuarantRequestBO[] requests, CancellationToken cancellationToken)
    {
        RestuarantBO[] requestCollection = new RestuarantBO[requests.Length];
        for (int i = 0; i < requests.Length; i++)
        {
            string newId = IdGenerator.GenerateId();
            RestuarantBO newItem = requests[i].MapToRestuarant(newId);
            requestCollection[i] = newItem;
        }

        _logger.LogInformation("Adding new restuarant");
        return await _repo.CreateManyRestuarantsAsync(requestCollection, cancellationToken);
    }

    /// <summary>
    /// Updates a Restuarant record
    /// </summary>
    /// <param name="id">Id of the restuarant</param>
    /// <param name="request">Restuarant properties to update</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Results of the update transaction</returns>
    public async Task<TransactionResult> UpdateRestuarantAsync(string id, UpdateRestuarantRequestBO request, CancellationToken cancellationToken)
    {
        _ = await GetRestuarantAsync(id, cancellationToken) ?? throw new NotFoundException("Restuarant does not exist.  Unable to update.", id);

        _logger.LogInformation("Updating restuarant");
        await _redis.RemoveCacheValueAsync(id, cancellationToken);
        return await _repo.UpdateRestuarantAsync(id, request, cancellationToken);
    }

    /// <summary>
    /// Update the location an existing restuarant
    /// </summary>
    /// <param name="id">Id of the restuarant</param>
    /// <param name="request">Restuarant location properties to update</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Results for the transaction</returns>
    public async Task<TransactionResult> UpdateRestuarantLocationAsync(string id, UpdateLocationRequestBO request, CancellationToken cancellationToken)
    {
        _ = await GetRestuarantAsync(id, cancellationToken) ?? throw new NotFoundException("Restuarant does not exist.  Unable to update.", id);

        _logger.LogInformation("Updating restuarant");
        await _redis.RemoveCacheValueAsync(id, cancellationToken);
        return await _repo.UpdateRestuarantLocationAsync(id, request, cancellationToken);
    }

    /// <summary>
    /// Removes a Restuarant record
    /// </summary>
    /// <param name="id">Id of the restuarant</param>
    /// <param name="cancellationToken">Token for handling cancellation requests</param>
    /// <returns>Success result</returns>
    public async Task<bool> RemoveRestuarantAsync(string id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing restuarant");
        await _redis.RemoveCacheValueAsync(id, cancellationToken);
        TransactionResult result = await _repo.RemoveRestuarantAsync(id, cancellationToken);
        return result.Success;
    }
}
