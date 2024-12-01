using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFCore.AuditBase.Interceptors;

internal class AuditPropertyValidationInterceptor : SaveChangesInterceptor
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
                           .Where(e => e.State == EntityState.Modified);

      foreach (var entry in entries)
      {
         var entityName = entry.Entity.GetType()
                               .Name;

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