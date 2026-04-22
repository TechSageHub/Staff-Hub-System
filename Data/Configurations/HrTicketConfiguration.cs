using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class HrTicketConfiguration : IEntityTypeConfiguration<HrTicket>
{
    public void Configure(EntityTypeBuilder<HrTicket> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Category)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(t => t.Subject)
            .IsRequired()
            .HasMaxLength(180);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(t => t.AdminComment)
            .HasMaxLength(800);

        builder.HasOne(t => t.Employee)
            .WithMany()
            .HasForeignKey(t => t.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
