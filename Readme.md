# Pandatech.EFCore.AuditBase

Pandatech.EFCore.AuditBase is a comprehensive auditing library for Entity Framework Core, designed to make entity
tracking, deletion management, and concurrency control straightforward and efficient.

## Features

- **Automatic Auditing**: Automatically tracks creation and update times, along with the corresponding user IDs.
- **Soft Delete Support**: Implements soft deletion logic, allowing entities to be marked as deleted without physical
  removal from the database.
- **Concurrency Control**: Integrates versioning to handle concurrency, minimizing data conflicts.
- **Enforced Best Practices**: Enforces the use of provided methods for updates and deletions, ensuring consistent audit
  trails.
- **Query Filter Management**: Allows for the exclusion of soft-deleted entities from queries, with an option to include
  them when necessary.
- **Seamless Integration**: Designed to integrate smoothly with EF Core projects, enhancing data integrity and
  compliance.

## Getting Started

To integrate Pandatech.EFCore.AuditBase into your project, install the NuGet package:

```bash
Install-Package Pandatech.EFCore.AuditBase
```

## Usage

1. Inherit from `AuditEntityBase` in your entity classes to enable auditing.
2. Use `MarkAsUpdated(userId)` and `MarkAsDeleted(userId)` methods to handle entity updates and deletions.
3. Apply `DbContextExtensions.UseAuditPropertyValidation` in your DbContext to enforce audit method usage.
4. Leverage `ModelBuilderExtensions.FilterOutDeletedMarkedObjects` to automatically exclude soft-deleted entities from
   EF Core queries.

### Entity Inheritance Example:

Assuming you have a `Product` entity in your application, you would inherit from `AuditEntityBase` to include auditing
properties:

```csharp
using Pandatech.VerticalSlices.Domain.Shared;

public class Product : AuditEntityBase
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

By inheriting from `AuditEntityBase`, Product automatically gains auditing properties
like `CreatedAt`, `CreatedByUserId`, `UpdatedAt`, `UpdatedByUserId`, `Deleted` and `Version`.

### Using `MarkAsUpdated` and `MarkAsDeleted`:

When updating or deleting an entity, use the provided methods to ensure the audit properties are correctly updated:

```csharp
public void UpdateProduct(Product product, long updatingUserId)
{
    // Perform your update logic...
    
    product.MarkAsUpdated(updatingUserId); //optional UpdatedAt DateTime parameter
    _dbContext.SaveChanges();
}

public void DeleteProduct(Product product, long deletingUserId)
{
    product.MarkAsDeleted(deletingUserId); //optional UpdatedAt DateTime parameter
    _dbContext.SaveChanges();
}
```

### DbContext Configuration:

In your `DbContext`, ensure you call `UseAuditPropertyValidation` to enforce the usage of audit methods and
`FilterOutDeletedMarkedObjects` to apply the global filter for soft-deleted entities:

```csharp
using Microsoft.EntityFrameworkCore;
using Pandatech.VerticalSlices.Infrastructure.Context;

public class MyDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.FilterOutDeletedMarkedObjects(); // Apply global query filter for soft deletes
    }

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
        this.UseAuditPropertyValidation(); // Enforce audit method usage
    }
}
```

### Using `ExecuteUpdateAndMarkUpdatedAsync`:

This extension method allows developers to update multiple properties while still maintaining audit consistency by
updating `UpdatedAt`, `UpdatedByUserId`, and `Version` behind the scenes.

```csharp
public async Task UpdateProductPricesAsync(MyDbContext dbContext, long userId)
{
    var expensiveProducts = dbContext.Products.Where(p => p.Price > 100);

    await expensiveProducts.ExecuteUpdateAndMarkUpdatedAsync(
        userId,
        x => x.SetProperty(p => p.Price, p => p.Price * 0.9) // Apply 10% discount
    );
}
```

### Using `ExecuteSoftDeleteAsync`:

To perform a soft delete on multiple entities, use the `ExecuteSoftDeleteAsync` extension method.

```csharp
public async Task SoftDeleteProductsAsync(MyDbContext dbContext, long userId)
{
    var productsToSoftDelete = dbContext.Products.Where(p => p.Price > 100);

    await productsToSoftDelete.ExecuteSoftDeleteAsync(userId);
}
```

### Ignoring Soft Delete Filter:

If you need to include soft-deleted entities in a specific query, use `IgnoreQueryFilters()`:

```csharp
var allProductsIncludingDeleted = _dbContext.Products.IgnoreQueryFilters().ToList();
```

## Handling Concurrency

The `AuditEntityBase` includes a versioning mechanism to manage concurrent updates. In the event of a conflict, a
concurrency exception will be thrown. This can be gracefully handled using try-catch blocks or integrated
with `Pandatech.ResponseCrafter` for automated response management.

However, `ExecuteUpdateAndMarkUpdatedAsync` and `ExecuteSoftDeleteAsync` **ignore optimistic locking**, meaning they
will not throw a concurrency exception if the `Version` field changes during the operation. This is because these
methods only increment the `Version` field by one, regardless of the current value.

## Contributing

Contributions are welcome! Please submit a pull request or open an issue to propose changes or report bugs.

## License

Pandatech.EFCore.AuditBase is licensed under the MIT License.