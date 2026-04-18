using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftBridge.Domain.Models.AccountAggregates;

namespace SoftBridge.Persistence.Configuration;

public class SProviderConfiguration : IEntityTypeConfiguration<SProvider>
{
    public void Configure(EntityTypeBuilder<SProvider> builder)
    {
        builder.HasKey(x => x.Id);

        builder.ToTable("Providers",
            t =>
            {
                t.HasCheckConstraint("CK_Providers_AverageRating", "[AverageRating] BETWEEN 0 AND 5");
                t.HasCheckConstraint("CK_Providers_TotalReviews", "[TotalReviews] >= 0");
            });

        builder.Property(x => x.Bio)
            .HasMaxLength(2000);

        builder.Property(x => x.ProfileImageUrl)
            .HasMaxLength(750);

        builder.Property(x => x.CvUrl)
            .HasMaxLength(750);

        builder.Property(x => x.PortfolioLink)
            .HasMaxLength(750);

        builder.Property(x => x.AverageRating)
            .HasDefaultValue(0f);

        builder.Property(x => x.TotalReviews)
            .HasDefaultValue(0);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAdd();

        builder.HasOne(x => x.User)
            .WithOne(x => x.ProviderProfile)
            .HasForeignKey<SProvider>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedByAdmin)
            .WithMany()
            .HasForeignKey(x => x.ApproveByAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => x.Status)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.ApproveByAdminId);
        builder.HasIndex(x => x.Status).HasFilter("[IsDeleted] = 0");
    }
}
