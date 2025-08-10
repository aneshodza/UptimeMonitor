using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Models;

namespace UptimeMonitor.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    // Reusable bool <-> int converter
    var boolToInt = new ValueConverter<bool, int>(
        v => v ? 1 : 0,   // to store
        v => v == 1       // to read
        );

    builder.Entity<User>(entity =>
        {
        entity.Property(e => e.EmailConfirmed)
        .HasColumnType("integer")
        .HasConversion(boolToInt);

        entity.Property(e => e.PhoneNumberConfirmed)
        .HasColumnType("integer")
        .HasConversion(boolToInt);

        entity.Property(e => e.TwoFactorEnabled)
        .HasColumnType("integer")
        .HasConversion(boolToInt);

        entity.Property(e => e.LockoutEnabled)
        .HasColumnType("integer")
        .HasConversion(boolToInt);
        });

    // If any of these are also wrong in your DB, map them too:
    // - LockoutEnd is normally timestamptz (DateTimeOffset?) in Postgres.
    // - AccessFailedCount is int already (leave as-is).
  }
}
