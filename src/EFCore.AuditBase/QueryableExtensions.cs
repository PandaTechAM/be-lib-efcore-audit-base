using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

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
    
    public static Task<int> ExecuteUpdateAndMarkUpdatedAsync<T>(
       this IQueryable<T> query,
       long? userId,
       Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setProperties,
       DateTime? updatedAt = null,
       CancellationToken cancellationToken = default)
       where T : AuditEntityBase
    {
       updatedAt ??= DateTime.UtcNow;
       
       var combinedProperties = (Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>)(x => 
             setProperties.Compile().Invoke(x)
                          .SetProperty(y => y.UpdatedAt, updatedAt)
                          .SetProperty(y => y.UpdatedByUserId, userId)
                          .SetProperty(y => y.Version, y => y.Version + 1)
          );

       return query.ExecuteUpdateAsync(combinedProperties, cancellationToken);
    }
    
    public static void MarkAsDeleted<T>(this IEnumerable<T> entities, long? userId, DateTime? updatedAt = null)
        where T : AuditEntityBase
    {
        updatedAt ??= DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.MarkAsDeleted(userId,updatedAt);
        }
    }
    
}