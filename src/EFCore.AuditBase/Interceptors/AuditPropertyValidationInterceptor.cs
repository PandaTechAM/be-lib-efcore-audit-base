using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFCore.AuditBase.Interceptors;

internal sealed class AuditPropertyValidationInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            ValidateAuditMethodUsage(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            ValidateAuditMethodUsage(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ValidateAuditMethodUsage(DbContext context)
    {
        var entries = context.ChangeTracker
            .Entries<AuditEntityBase>()
            .Where(e => e.State == EntityState.Modified)
            .ToList();

        if (entries.Count is 0)
        {
            return;
        }

        if (entries.Any(x => x.Entity.IgnoreInterceptor))
        {
            return;
        }

        foreach (var entry in entries)
        {
            var originalVersion = entry.OriginalValues[nameof(AuditEntityBase.Version)] as int?;
            var currentVersion = entry.CurrentValues[nameof(AuditEntityBase.Version)] as int?;

            if (originalVersion == currentVersion)
            {
                throw new InvalidOperationException(
                    $"Entity '{entry.Entity.GetType().Name}' was modified without calling MarkAsUpdated or MarkAsDeleted. " +
                    "All modifications to audited entities must go through the provided audit methods.");
            }
        }
    }
}
