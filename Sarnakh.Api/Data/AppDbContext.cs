using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sarnakh.Api.Models;

namespace Sarnakh.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Process>().ToTable("Processes", "public");
        builder.Entity<Lead>().ToTable("Leads", "public");
        builder.Entity<Keyword>().ToTable("Keywords", "public");
        builder.Entity<Config>().ToTable("Configs", "public");
        builder.Entity<KeywordPack>().ToTable("KeywordPacks", "public");
        builder.Entity<TelegramGroupCategory>().ToTable("TelegramGroupCategories", "public");
        builder.Entity<TelegramGroupCategory>()
            .Property(x => x.GroupIds)
            .HasColumnType("bigint[]");

        builder.Entity<Process>()
            .HasOne(p => p.Config)
            .WithMany()
            .HasForeignKey(p => p.ConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Lead>()
            .HasOne<Process>()
            .WithMany()
            .HasForeignKey(l => l.ProcessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Keyword>()
            .HasOne(k => k.Config)
            .WithMany(c => c.Keywords)
            .HasForeignKey(k => k.ConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Keyword>()
            .HasOne(k => k.Config)
            .WithMany(c => c.Keywords)
            .HasForeignKey(k => k.ConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        // Store enum as string, not native PG enum
        builder.Entity<Process>()
            .Property(p => p.ProcessStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasDefaultSchema("identity");
        builder.Entity<TestModel>().ToTable("TestModels", "public");
        builder.Entity<RefreshToken>().ToTable("RefreshTokens", "identity");
    }

    public DbSet<TestModel> TestModels => Set<TestModel>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Process> Processes => Set<Process>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Keyword> Keywords => Set<Keyword>();
    public DbSet<Config> Configs => Set<Config>();
    public DbSet<TelegramGroupCategory> TelegramGroupCategories => Set<TelegramGroupCategory>();
    public DbSet<KeywordPack> KeywordPacks => Set<KeywordPack>();
}
