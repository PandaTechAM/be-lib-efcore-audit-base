using System.ComponentModel.DataAnnotations;

namespace EFCore.AuditBase;

public abstract class AuditEntityBase
{
   public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   public required long? CreatedByUserId { get; init; }
   public DateTime? UpdatedAt { get; private set; }
   public long? UpdatedByUserId { get; private set; }
   public bool Deleted { get; private set; }

   [ConcurrencyCheck]
   public int Version { get; private set; } = 1;

   public void MarkAsUpdated(long? userId, DateTime? updatedAt = null)
   {
      UpdatedAt = updatedAt ?? DateTime.UtcNow;
      UpdatedByUserId = userId;
      Version++;
   }

   public void MarkAsDeleted(long? userId, DateTime? updatedAt = null)
   {
      Deleted = true;
      UpdatedAt = updatedAt ?? DateTime.UtcNow;
      UpdatedByUserId = userId;
      Version++;
   }
   public void SyncAuditBase(AuditEntityBase source)
   {
      UpdatedAt = source.UpdatedAt;
      UpdatedByUserId = source.UpdatedByUserId;
      Deleted = source.Deleted;
      Version = source.Version;
   }
}