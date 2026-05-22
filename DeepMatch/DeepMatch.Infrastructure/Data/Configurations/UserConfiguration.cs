using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeepMatch.Domain.Constants;
using DeepMatch.Domain.Entities;

namespace DeepMatch.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(BusinessRules.Users.EmailMaxLength);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(BusinessRules.Users.UserNameMaxLength);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(BusinessRules.Users.PasswordHashMaxLength);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(BusinessRules.Users.RoleMaxLength)
            .HasDefaultValue(UserRoles.User);

        builder.Property(u => u.Bio)
            .HasMaxLength(BusinessRules.Users.BioMaxLength);

        builder.Property(u => u.ReportsCount)
            .HasDefaultValue(0);

        builder.Property(u => u.BlockReason)
            .HasMaxLength(BusinessRules.Reports.MaxReasonLength);

        builder.OwnsOne(u => u.Rating, rating =>
        {
            rating.Property(r => r.Value).HasColumnName("Rating");
        });

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
