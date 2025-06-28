using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<EmployeeAddress>
    {
        public void Configure(EntityTypeBuilder<EmployeeAddress> builder)
        {
            builder.HasKey(a => a.Id);

            // One-to-one: One employee has one address
            builder.HasOne(a => a.Employee)
                   .WithOne(e => e.Address)
                   .HasForeignKey<EmployeeAddress>(a => a.EmployeeId)
                   .OnDelete(DeleteBehavior.Cascade); // if employee is deleted, delete address

            builder.Property(a => a.Street).HasMaxLength(100);
            builder.Property(a => a.City).HasMaxLength(50);
            builder.Property(a => a.State).HasMaxLength(50);
            builder.Property(a => a.Country).HasMaxLength(50);
        }
    }
}



