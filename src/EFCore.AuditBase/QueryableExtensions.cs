using Microsoft.EntityFrameworkCore;

namespace EFCore.AuditBase;

/// <summary>Bulk soft-delete helpers for audited entities.</summary>
public static class QueryableExtensions
{
    /// <summary>
    ///     Set-based soft delete via a single UPDATE (no entities loaded): marks matching rows deleted,
    ///     stamps the update user/timestamp, and increments the version.
    /// </summary>
    public static Task<int> ExecuteSoftDeleteAsync<T>(this IQueryable<T> query,
        long? userId,
        DateTime? updatedAt = null,
        CancellationToken ct = default)
        where T : AuditEntityBase
    {
        updatedAt ??= DateTime.UtcNow;


        return query.ExecuteUpdateAsync(x => x
                .SetProperty(y => y.Deleted, true)
                .SetProperty(y => y.UpdatedAt, updatedAt)
                .SetProperty(y => y.UpdatedByUserId, userId)
                .SetProperty(y => y.Version, y => y.Version + 1),
            ct);
    }

    /// <summary>
    ///     Mark every entity in the sequence as soft-deleted in memory (each tracked change goes through the
    ///     audit methods).
    /// </summary>
    public static void MarkAsDeleted<T>(this IEnumerable<T> entities, long? userId, DateTime? updatedAt = null)
        where T : AuditEntityBase
    {
        updatedAt ??= DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.MarkAsDeleted(userId, updatedAt);
        }
    }
}
