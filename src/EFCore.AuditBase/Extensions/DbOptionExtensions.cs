using EFCore.AuditBase.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace EFCore.AuditBase.Extensions;

/// <summary>Registration helpers for the audit-base interceptor.</summary>
public static class OptionsBuilderExtensions
{
    /// <summary>
    ///     Register the interceptor that enforces audit-method usage: SaveChanges throws if an audited entity
    ///     was modified without calling MarkAsUpdated or MarkAsDeleted.
    /// </summary>
    public static DbContextOptionsBuilder UseAuditBaseValidatorInterceptor(this DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new AuditPropertyValidationInterceptor());

        return optionsBuilder;
    }
}
