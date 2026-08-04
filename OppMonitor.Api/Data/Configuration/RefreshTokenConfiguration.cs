using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OppMonitor.Api.Models;

namespace OppMonitor.Api.Data.Configuration;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Token).HasMaxLength(200);
        builder.HasIndex(r => r.Token).IsUnique();

        builder
            .HasOne(r => r.ApplicationUser)
            .WithMany()
            .HasForeignKey(r => r.UserId);
    }
}
