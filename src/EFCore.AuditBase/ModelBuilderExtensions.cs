using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace EFCore.AuditBase;

/// <summary>Model-building helpers for audited entities.</summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    ///     Add a global query filter to every <see cref="AuditEntityBase" />-derived entity that excludes
    ///     soft-deleted rows from all queries.
    /// </summary>
    public static void FilterOutDeletedMarkedObjects(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(AuditEntityBase).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var propertyInfo = entityType.ClrType.GetProperty(nameof(AuditEntityBase.Deleted));

            if (propertyInfo is null)
            {
                throw new InvalidOperationException(
                    $"Expected property '{nameof(AuditEntityBase.Deleted)}' not found on '{entityType.ClrType.Name}'.");
            }

            var propertyAccess = Expression.MakeMemberAccess(parameter, propertyInfo);

            var notExpression = Expression.Not(propertyAccess);
            var lambda = Expression.Lambda(notExpression, parameter);

            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(lambda);
        }
    }
}
