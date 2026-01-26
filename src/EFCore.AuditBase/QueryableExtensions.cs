using Microsoft.EntityFrameworkCore;

namespace EFCore.AuditBase;

public static class QueryableExtensions
{
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