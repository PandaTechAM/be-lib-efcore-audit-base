using Microsoft.EntityFrameworkCore;

public class InMemoryContext(DbContextOptions<InMemoryContext> options) : DbContext(options)
{
   public DbSet<Product> Products => Set<Product>();
}