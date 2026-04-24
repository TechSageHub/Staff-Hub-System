using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class OffboardingChecklistItemConfiguration : IEntityTypeConfiguration<OffboardingChecklistItem>
{
    public void Configure(EntityTypeBuilder<OffboardingChecklistItem> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Key)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(m => m.Key)
            .IsUnique();
    }
}
