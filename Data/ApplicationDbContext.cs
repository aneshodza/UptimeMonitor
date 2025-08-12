using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Models;
using System.Security.Cryptography;

namespace UptimeMonitor.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<ScheduledCheck> ScheduledChecks { get; set; }
    public DbSet<Check> Checks { get; set; }
    public DbSet<Group> Groups { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Reusable bool <-> int converter
        var boolToInt = new ValueConverter<bool, int>(v => v ? 1 : 0, // to store
                                                      v => v == 1     // to read
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

        builder.Entity<ScheduledCheck>(entity =>
        {
            entity.HasIndex(e => e.Domain).IsUnique();
            entity.Property(x => x.Domain).IsRequired().HasMaxLength(255);
        });

        builder.Entity<Check>(entity =>
        {
            entity.HasOne(c => c.ScheduledCheck)
                .WithMany(sc => sc.Checks)
                .HasForeignKey(c => c.ScheduledCheckId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Group>(entity =>
        {
            entity.Property(g => g.GroupCode).IsRequired().HasMaxLength(8);

            entity.HasIndex(g => g.GroupCode).IsUnique();

            entity.HasOne(g => g.Owner)
                .WithMany(u => u.OwnedGroups)
                .HasForeignKey(g => g.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(g => g.Members)
                .WithMany(u => u.JoinedGroups)
                .UsingEntity<Dictionary<string, object>>(
                    "GroupMember",
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId").OnDelete(
                        DeleteBehavior.Cascade),
                    j => j.HasOne<Group>()
                             .WithMany()
                             .HasForeignKey("GroupId")
                             .OnDelete(DeleteBehavior.Cascade));
        });
    }

    public override async Task<int>
    SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var e in ChangeTracker.Entries<Group>().Where(
                     e => e.State == EntityState.Added))
        {
            if (string.IsNullOrWhiteSpace(e.Entity.GroupCode))
                e.Entity.GroupCode = GenerateGroupCode(8);

            if (!string.IsNullOrEmpty(e.Entity.OwnerId))
            {
                e.Entity.Members ??= new List<User>();

                if (!e.Entity.Members.Any(m => m.Id == e.Entity.OwnerId))
                {
                    var ownerStub = new User { Id = e.Entity.OwnerId };
                    Attach(ownerStub);
                    e.Entity.Members.Add(ownerStub);
                }
            }
        }

        return await base.SaveChangesAsync(ct);
    }

    private static string GenerateGroupCode(int length)
    {
        const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
        var bytes = RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = alphabet[bytes[i] % alphabet.Length];
        return new string(chars);
    }
}
