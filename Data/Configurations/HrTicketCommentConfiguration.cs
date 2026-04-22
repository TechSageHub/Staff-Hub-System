using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class HrTicketCommentConfiguration : IEntityTypeConfiguration<HrTicketComment>
{
    public void Configure(EntityTypeBuilder<HrTicketComment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CommenterName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasOne(c => c.HrTicket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.HrTicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
