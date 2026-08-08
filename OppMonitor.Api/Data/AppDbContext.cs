using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OppMonitor.Api.Models;

namespace OppMonitor.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Process>().ToTable("Processes", "public");
        builder.Entity<Lead>().ToTable("Leads", "public");
        builder.Entity<Config>().ToTable("Configs", "public");

        builder.Entity<Process>()
            .HasOne(p => p.Config)
            .WithMany()
            .HasForeignKey(p => p.ConfigId)
            .OnDelete(DeleteBehavior.Restrict);

        // Store enum as string, not native PG enum
        builder.Entity<Process>()
            .Property(p => p.ProcessStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Entity<Config>()
            .Property(c => c.Keywords)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, (JsonSerializerOptions?)null) ?? new())
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, int>>(
                (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                d => d.Aggregate(0, (h, kv) => HashCode.Combine(h, kv.Key.GetHashCode(), kv.Value)),
                d => new Dictionary<string, int>(d)));

        builder.Entity<Config>()
            .Property(c => c.TelegramChatIds)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<long>>(v, (JsonSerializerOptions?)null) ?? new())
            .Metadata.SetValueComparer(new ValueComparer<List<long>>(
                (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                l => l.Aggregate(0, HashCode.Combine),
                l => l.ToList()));

        builder.HasDefaultSchema("identity");
        builder.Entity<TestModel>().ToTable("TestModels", "public");
        builder.Entity<RefreshToken>().ToTable("RefreshTokens", "identity");
    }

    public DbSet<TestModel> TestModels => Set<TestModel>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Process> Processes => Set<Process>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Config> Configs => Set<Config>();
}
