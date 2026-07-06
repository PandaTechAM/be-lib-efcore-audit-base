using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.AuditBase;

/// <summary>
///     Base class for audited entities: tracks creation/update user and timestamps, soft delete, and an
///     optimistic-concurrency version. Mutations must go through the Mark* methods, enforced by the audit interceptor.
/// </summary>
public abstract class AuditEntityBase
{
    [NotMapped]
    internal bool IgnoreInterceptor;

    /// <summary>UTC timestamp when the entity was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Id of the user who created the entity; null for system-created records.</summary>
    public required long? CreatedByUserId { get; init; }

    /// <summary>UTC timestamp of the last audited update; null if never updated.</summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>Id of the user who performed the last audited update.</summary>
    public long? UpdatedByUserId { get; private set; }

    /// <summary>Whether the entity has been soft-deleted.</summary>
    public bool Deleted { get; private set; }

    /// <summary>Optimistic-concurrency version, incremented on every audited change.</summary>
    [ConcurrencyCheck]
    public int Version { get; private set; } = 1;

    /// <summary>Record an audited update: sets the update user/timestamp and increments the version.</summary>
    public void MarkAsUpdated(long? userId, DateTime? updatedAt = null)
    {
        UpdatedAt = updatedAt ?? DateTime.UtcNow;
        UpdatedByUserId = userId;
        Version++;
    }

    /// <summary>Mark the entity as soft-deleted: sets the update user/timestamp and increments the version.</summary>
    public void MarkAsDeleted(long? userId, DateTime? updatedAt = null)
    {
        Deleted = true;
        UpdatedAt = updatedAt ?? DateTime.UtcNow;
        UpdatedByUserId = userId;
        Version++;
    }

    /// <summary>
    ///     Copy the audit state (update fields, deleted flag, version) from another instance, bypassing the
    ///     audit-method validation interceptor for this entity.
    /// </summary>
    public void SyncAuditBase(AuditEntityBase source)
    {
        IgnoreInterceptor = true;
        UpdatedAt = source.UpdatedAt;
        UpdatedByUserId = source.UpdatedByUserId;
        Deleted = source.Deleted;
        Version = source.Version;
    }
}
