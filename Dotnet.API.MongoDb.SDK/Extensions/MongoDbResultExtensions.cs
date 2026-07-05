using Dotnet.API.MongoDb.SDK.Constants;
using Dotnet.API.SDK.Models;
using MongoDB.Driver;

namespace Dotnet.API.MongoDb.SDK.Extensions;

internal static class MongoDbResultExtensions
{
    internal static TransactionResult ToTransactionResult(this ReplaceOneResult result, long expectedRecordCount = 1, bool transactionRun = true)
    {
        return new TransactionResult
        (
            transactionRun,
            result.IsAcknowledged,
            expectedRecordCount,
            result.ModifiedCount,
            DatabaseConstants.Updated
        );
    }

    internal static TransactionResult ToTransactionResult(this UpdateResult result, long expectedRecordCount = 1, bool transactionRun = true)
    {
        return new TransactionResult
        (
            transactionRun,
            result.IsAcknowledged,
            expectedRecordCount,
            result.ModifiedCount,
            DatabaseConstants.Updated
        );
    }

    internal static TransactionResult ToTransactionResult(this DeleteResult result, long expectedRecordCount, bool transactionRun = true)
    {
        return new TransactionResult
        (
            transactionRun,
            result.IsAcknowledged,
            expectedRecordCount,
            result.DeletedCount,
            DatabaseConstants.Deleted
        );
    }
}
