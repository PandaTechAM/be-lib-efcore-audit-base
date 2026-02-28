# Pandatech.EFCore.AuditBase

Auditing base for EF Core entities. Inherit one class and get automatic `CreatedAt`/`UpdatedAt`/`UserId` tracking,
soft delete, optimistic concurrency via row versioning, bulk update/delete helpers, and a `SaveChanges` interceptor
that enforces correct audit method usage at runtime.

Targets **`net8.0`**, **`net9.0`**, and **`net10.0`**.

---

## Table of Contents

1. [Features](#features)
2. [Installation](#installation)
3. [Getting Started](#getting-started)
4. [AuditEntityBase](#auditentitybase)
5. [Registering the Interceptor](#registering-the-interceptor)
6. [Soft Delete Query Filter](#soft-delete-query-filter)
7. [Bulk Operations](#bulk-operations)
8. [Concurrency Handling](#concurrency-handling)
9. [SyncAuditBase](#syncauditbase)

---

## Features

- **Automatic audit fields** — `CreatedAt`, `CreatedByUserId`, `UpdatedAt`, `UpdatedByUserId`, `Deleted`, `Version`
  maintained on every entity that inherits `AuditEntityBase`
- **Enforced audit methods** — a `SaveChanges` interceptor throws at runtime if a modified entity's `Version` was not
  incremented, meaning someone bypassed `MarkAsUpdated`/`MarkAsDeleted`
- **Soft delete** — `Deleted` flag with `MarkAsDeleted` and a global query filter that hides deleted rows transparently
- **Optimistic concurrency** — `Version` is decorated with `[ConcurrencyCheck]`; EF Core raises a concurrency
  exception on conflict automatically
- **Bulk helpers** — `ExecuteSoftDeleteAsync` and `ExecuteUpdateAndMarkUpdatedAsync` translate directly to
  `ExecuteUpdateAsync` database calls while still maintaining correct audit fields
- **In-memory batch delete** — `MarkAsDeleted` overload on `IEnumerable<T>` for cases where entities are already
  tracked

---

## Installation

```bash
dotnet add package Pandatech.EFCore.AuditBase
```

---

## Getting Started

Inherit `AuditEntityBase` in your entity:

```csharp
public class Product : AuditEntityBase
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

That's enough to gain all audit properties. Wire up the interceptor and query filter as shown below.

---

## AuditEntityBase

```
CreatedAt          DateTime       Set to UtcNow on construction. Never modified after that.
CreatedByUserId    long?          Required on construction (required init). Never modified after that.
UpdatedAt          DateTime?      Set by MarkAsUpdated / MarkAsDeleted.
UpdatedByUserId    long?          Set by MarkAsUpdated / MarkAsDeleted.
Deleted            bool           Set to true by MarkAsDeleted.
Version            int            Starts at 1. Incremented by every MarkAsUpdated / MarkAsDeleted call.
```

### MarkAsUpdated

```csharp
product.MarkAsUpdated(userId);
// or with an explicit timestamp:
product.MarkAsUpdated(userId, updatedAt: syncedTime);

await dbContext.SaveChangesAsync(ct);
```

### MarkAsDeleted

```csharp
product.MarkAsDeleted(userId);
await dbContext.SaveChangesAsync(ct);
```

Both methods increment `Version`. The interceptor validates this increment on every `SaveChanges` call — if you modify
an audited entity's properties directly without calling `MarkAsUpdated`, the interceptor throws:

```
InvalidOperationException: Entity 'Product' was modified without calling MarkAsUpdated or MarkAsDeleted.
```

---

## Registering the Interceptor

```csharp
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseAuditBaseValidatorInterceptor());
```

`UseAuditBaseValidatorInterceptor` adds `AuditPropertyValidationInterceptor` to the context. It hooks into both
`SavingChanges` and `SavingChangesAsync`.

---

## Soft Delete Query Filter

Apply a global query filter in `OnModelCreating` to exclude soft-deleted rows from all queries automatically:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.FilterOutDeletedMarkedObjects();
}
```

`FilterOutDeletedMarkedObjects` iterates every entity type that inherits `AuditEntityBase` and applies
`.HasQueryFilter(e => !e.Deleted)` to each one via expression trees.

To include deleted rows in a specific query:

```csharp
var all = await dbContext.Products.IgnoreQueryFilters().ToListAsync(ct);
```

---

## Bulk Operations

### ExecuteSoftDeleteAsync

Soft-deletes all rows matching a query in a single `UPDATE` statement. Does not load entities into memory.

```csharp
await dbContext.Products
    .Where(p => p.Price > 100)
    .ExecuteSoftDeleteAsync(userId, ct: ct);
```

Translates to:

```sql
UPDATE products
SET deleted = true, updated_at = NOW(), updated_by_user_id = @userId, version = version + 1
WHERE price > 100
```

### ExecuteUpdateAndMarkUpdatedAsync

Updates arbitrary properties while automatically maintaining `UpdatedAt`, `UpdatedByUserId`, and `Version`:

```csharp
await dbContext.Products
    .Where(p => p.Price > 100)
    .ExecuteUpdateAndMarkUpdatedAsync(
        userId,
        x => x.SetProperty(p => p.Price, p => p.Price * 0.9m),
        ct);
```

### MarkAsDeleted (in-memory batch)

For already-tracked entities where you want to call `SaveChanges` once after marking several:

```csharp
var products = await dbContext.Products.Where(p => p.Price > 100).ToListAsync(ct);
products.MarkAsDeleted(userId);
await dbContext.SaveChangesAsync(ct);
```

> **Optimistic locking note:** `ExecuteSoftDeleteAsync` and `ExecuteUpdateAndMarkUpdatedAsync` bypass EF Core's
> change tracker and do not raise concurrency exceptions. They increment `Version` unconditionally. Use them when
> bulk throughput matters more than per-row conflict detection.

---

## Concurrency Handling

`Version` is decorated with `[ConcurrencyCheck]`. When two requests load the same entity and both call `SaveChanges`,
the second one gets a `DbUpdateConcurrencyException` because the `Version` in the database no longer matches what was
read.

```csharp
try
{
    product.MarkAsUpdated(userId);
    await dbContext.SaveChangesAsync(ct);
}
catch (DbUpdateConcurrencyException)
{
    // Reload and retry, or return a 409 to the caller.
}
```

---

## SyncAuditBase

`SyncAuditBase` copies audit fields from one entity instance to another while bypassing the interceptor. It is
intended for internal synchronization scenarios (e.g., merging detached entity state) and should not be used in
normal update flows.

```csharp
target.SyncAuditBase(source);
await dbContext.SaveChangesAsync(ct);
```

Setting `IgnoreInterceptor = true` via `SyncAuditBase` suppresses validation for all modified entities in that
`SaveChanges` call, so use it only when you are certain the audit state being applied is already correct.

---

## License

MIT