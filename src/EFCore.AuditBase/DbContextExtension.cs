using Microsoft.EntityFrameworkCore;

namespace EFCore.AuditBase;

public static class DbContextExtensions
{
    public static void UseAuditPropertyValidation(this DbContext context)
    {
        context.SavingChanges += (sender, e) =>
        {
            var dbContext = (DbContext)sender!;
            ValidateAuditMethodUsage(dbContext);
        };
    }

    private static void ValidateAuditMethodUsage(DbContext context)
    {
        var entries = context.ChangeTracker.Entries<AuditEntityBase>()
            .Where(e => e.State is EntityState.Modified);

        foreach (var entry in entries)
        {
            var entityName = entry.Entity.GetType().Name;

            if (entry.State is not EntityState.Modified)
            {
                continue;
            }

            var originalVersion = entry.OriginalValues[nameof(AuditEntityBase.Version)] as int?;
            var currentVersion = entry.CurrentValues[nameof(AuditEntityBase.Version)] as int?;

            if (originalVersion == currentVersion)
            {
                throw new InvalidOperationException(
                    $"Entity '{entityName}' must be updated using MarkAsUpdated method. Missing or incorrect audit fields for update.");

            }
        }
    }
}