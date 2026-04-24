using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class EmployeeOffboardingProgressConfiguration : IEntityTypeConfiguration<EmployeeOffboardingProgress>
{
    public void Configure(EntityTypeBuilder<EmployeeOffboardingProgress> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(p => new { p.EmployeeOffboardingId, p.OffboardingChecklistItemId })
            .IsUnique();

        builder.HasOne(p => p.EmployeeOffboarding)
            .WithMany(o => o.ProgressEntries)
            .HasForeignKey(p => p.EmployeeOffboardingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.OffboardingChecklistItem)
            .WithMany(m => m.ProgressEntries)
            .HasForeignKey(p => p.OffboardingChecklistItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
