using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class EmployeeOffboardingConfiguration : IEntityTypeConfiguration<EmployeeOffboarding>
{
    public void Configure(EntityTypeBuilder<EmployeeOffboarding> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Reason)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(o => o.Notes)
            .HasMaxLength(2000);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(500);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(o => o.EmployeeId)
            .IsUnique();

        builder.HasOne(o => o.Employee)
            .WithOne(e => e.Offboarding)
            .HasForeignKey<EmployeeOffboarding>(o => o.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
