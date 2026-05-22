using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Infrastructure.Data.Configurations;

public class SwipeConfiguration : IEntityTypeConfiguration<Swipe>
{
    public void Configure(EntityTypeBuilder<Swipe> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Direction)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(BusinessRules.Swipes.DirectionMaxLength);

        builder.HasOne(s => s.SwiperUser)
            .WithMany(u => u.Swipes)
            .HasForeignKey(s => s.SwiperUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.TargetAnswer)
            .WithMany(a => a.Swipes)
            .HasForeignKey(s => s.TargetAnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.SwiperUserId, s.TargetAnswerId }).IsUnique();
    }
}
