using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

        builder.Entity<Process>()
            .Property(p => p.Config)
            .HasColumnType("jsonb");

        // builder.Entity<ApplicationUser>(entity =>
        //         entity.Property(e => e.EnableNotifications).HasDefaultValue(true));
        builder.HasDefaultSchema("identity");
        builder.Entity<TestModel>().ToTable("TestModels", "public");
        builder.Entity<RefreshToken>().ToTable("RefreshTokens", "identity");
    }

    public DbSet<TestModel> TestModels => Set<TestModel>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Process> Processes => Set<Process>();
    public DbSet<Lead> Leads => Set<Lead>();
}
