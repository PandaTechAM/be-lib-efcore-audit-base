using EFCore.AuditBase;
using EFCore.AuditBase.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InMemoryContext>(options =>
   options.UseInMemoryDatabase("DemoDb")
          .UseAuditBaseValidatorInterceptor());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/products",
   async ([FromServices] InMemoryContext db) =>
   {
      var product = new Product
      {
         CreatedAt = DateTime.Now,
         CreatedByUserId = 23,
         Name = "asd",
         Price = 23
      };

      db.Add(product);
      await db.SaveChangesAsync();

      product.Name = "asd2";
      product.Price = 24;

      product.MarkAsUpdated(24, DateTime.Now);
      await db.SaveChangesAsync();

      var product2 = new Product
      {
         CreatedAt = product.CreatedAt,
         CreatedByUserId = product.CreatedByUserId,
         Name = product.Name,
         Price = product.Price,
      };
      product2.SyncAuditBase(product);

      db.Add(product2);

      await db.SaveChangesAsync();

      return TypedResults.Ok(product);
   });

app.Run();

public class Product : AuditEntityBase
{
   public int Id { get; set; }
   public required string Name { get; set; }
   public decimal Price { get; set; }
}