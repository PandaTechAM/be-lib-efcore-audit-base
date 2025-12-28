using EFCore.AuditBase.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace EFCore.AuditBase.Extensions;

public static class OptionsBuilderExtensions
{
   public static DbContextOptionsBuilder UseAuditBaseValidatorInterceptor(this DbContextOptionsBuilder optionsBuilder)
   {
      optionsBuilder.AddInterceptors(new AuditPropertyValidationInterceptor());

      return optionsBuilder;
   }
}