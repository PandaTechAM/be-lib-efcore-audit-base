using Microsoft.EntityFrameworkCore;

namespace EFCore.AuditBase;

public static class QueryableExtensions
{
    public static Task<int> ExecuteSoftDeleteAsync<T>(this IQueryable<T> query, long? userId,
        DateTime? updatedAt = null,
        CancellationToken cancellationToken = default)
        where T : AuditEntityBase
    {
        updatedAt ??= DateTime.UtcNow;


        return query.ExecuteUpdateAsync(x => x
            .SetProperty(y => y.Deleted, true)
            .SetProperty(y => y.UpdatedAt, updatedAt)
            .SetProperty(y => y.UpdatedByUserId, userId)
            .SetProperty(y => y.Version, y => y.Version + 1), cancellationToken);
    }
}