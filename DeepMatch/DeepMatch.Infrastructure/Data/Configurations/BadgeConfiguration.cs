using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Infrastructure.Data.Configurations;

public class BadgeConfiguration : IEntityTypeConfiguration<Badge>
{
    public void Configure(EntityTypeBuilder<Badge> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(BusinessRules.Badges.NameMaxLength);

        builder.Property(b => b.Description)
            .IsRequired()
            .HasMaxLength(BusinessRules.Badges.DescriptionMaxLength);

        builder.Property(b => b.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(BusinessRules.Badges.TypeMaxLength);

        builder.HasMany(b => b.Users)
            .WithMany(u => u.Badges)
            .UsingEntity<Dictionary<string, object>>(
                "UserBadge",
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                j => j.HasOne<Badge>().WithMany().HasForeignKey("BadgeId"),
                j => j.HasKey("UserId", "BadgeId")
            );
    }
}
