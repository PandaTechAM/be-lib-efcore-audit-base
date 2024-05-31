- [1. Pandatech.EFCore.AuditBase](#1-pandatechefcoreauditbase)
  - [1.1. Features](#11-features)
  - [1.2. Getting Started](#12-getting-started)
  - [1.3. Usage](#13-usage)
    - [1.3.1. Entity Inheritance Example:](#131-entity-inheritance-example)
    - [1.3.2. Using `MarkAsUpdated` and `MarkAsDeleted`:](#132-using-markasupdated-and-markasdeleted)
    - [1.3.3. DbContext Configuration:](#133-dbcontext-configuration)
    - [1.3.4. Ignoring Soft Delete Filter:](#134-ignoring-soft-delete-filter)
  - [1.4. Handling Concurrency](#14-handling-concurrency)
  - [1.5. Contributing](#15-contributing)
  - [1.6. License](#16-license)

# 1. Pandatech.EFCore.AuditBase

Pandatech.EFCore.AuditBase is a comprehensive auditing library for Entity Framework Core, designed to make entity
tracking, deletion management, and concurrency control straightforward and efficient.

## 1.1. Features

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

## 1.2. Getting Started

To integrate Pandatech.EFCore.AuditBase into your project, install the NuGet package:

```bash
Install-Package Pandatech.EFCore.AuditBase
```

## 1.3. Usage

1. Inherit from `AuditEntityBase` in your entity classes to enable auditing.
2. Use `MarkAsUpdated(userId)` and `MarkAsDeleted(userId)` methods to handle entity updates and deletions.
3. Apply `DbContextExtensions.UseAuditPropertyValidation` in your DbContext to enforce audit method usage.
4. Leverage `ModelBuilderExtensions.FilterOutDeletedMarkedObjects` to automatically exclude soft-deleted entities from
   EF Core queries.

### 1.3.1. Entity Inheritance Example:

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

By inheriting from AuditEntityBase, Product automatically gains auditing properties
like `CreatedAt`, `CreatedByUserId`, `UpdatedAt`, `UpdatedByUserId`, `Deleted` and `Version`.

### 1.3.2. Using `MarkAsUpdated` and `MarkAsDeleted`:

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

### 1.3.3. DbContext Configuration:

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

### 1.3.4. Ignoring Soft Delete Filter:

If you need to include soft-deleted entities in a specific query, use `IgnoreQueryFilters()`:

```csharp
var allProductsIncludingDeleted = _dbContext.Products.IgnoreQueryFilters().ToList();
```

## 1.4. Handling Concurrency

The `AuditEntityBase` includes a versioning mechanism to manage concurrent updates. In the event of a conflict, a
concurrency exception will be thrown. This can be gracefully handled using try-catch blocks or integrated
with `Pandatech.ResponseCrafter` for automated response management. This concurrency control is enabled by default and
is not locking the row as it uses optimistic lock.

## 1.5. Contributing

Contributions are welcome! Please submit a pull request or open an issue to propose changes or report bugs.

## 1.6. License

Pandatech.EFCore.AuditBase is licensed under the MIT License.