using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Infrastructure.Data.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.HasKey(m => m.Id);

        builder.HasOne(m => m.User1)
            .WithMany(u => u.MatchesAsUser1)
            .HasForeignKey(m => m.User1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.User2)
            .WithMany(u => u.MatchesAsUser2)
            .HasForeignKey(m => m.User2Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.CatalystAnswer1)
            .WithMany()
            .HasForeignKey(m => m.CatalystAnswer1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.CatalystAnswer2)
            .WithMany()
            .HasForeignKey(m => m.CatalystAnswer2Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.User1Id, m.User2Id }).IsUnique();
    }
}
