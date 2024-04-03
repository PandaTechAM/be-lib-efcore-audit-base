using System.ComponentModel.DataAnnotations;

namespace EFCore.AuditBase;

public abstract class AuditEntityBase
{
   public DateTime CreatedAt { get; private init; } = DateTime.UtcNow;
   public required long? CreatedByUserId { get; init; }
   public DateTime? UpdatedAt { get; private set; }
   public long? UpdatedByUserId { get; private set; }
   public bool Deleted { get; private set; }
   [ConcurrencyCheck] public int Version { get; private set; } = 1;
   
   public void MarkAsUpdated(long? userId)
   {
      this.UpdatedAt = DateTime.UtcNow;
      this.UpdatedByUserId = userId;
      this.Version++;
   }

   public void MarkAsDeleted(long? userId)
   {
      this.Deleted = true;
      this.UpdatedAt = DateTime.UtcNow;
      this.UpdatedByUserId = userId;
      this.Version++;
   }
}
