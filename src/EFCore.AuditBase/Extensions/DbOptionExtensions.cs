using Microsoft.EntityFrameworkCore;
using EFCore.AuditBase.Interceptors;

namespace EFCore.AuditBase.Extensions;

public static class OptionsBuilderExtensions
{
   public static DbContextOptionsBuilder UseAuditBaseValidatorInterceptor(this DbContextOptionsBuilder optionsBuilder)
   {
      optionsBuilder.AddInterceptors(new AuditPropertyValidationInterceptor());

      return optionsBuilder;
   }
}