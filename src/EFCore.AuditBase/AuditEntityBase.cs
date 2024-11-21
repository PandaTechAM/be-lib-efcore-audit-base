using System.ComponentModel.DataAnnotations;

namespace EFCore.AuditBase;

public abstract class AuditEntityBase
{
   [Required]
   public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   public required long? CreatedByUserId { get; init; }
   public DateTime? UpdatedAt { get; private set; }
   public long? UpdatedByUserId { get; private set; }

   [Required]
   public bool Deleted { get; private set; }

   [Required]
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
}