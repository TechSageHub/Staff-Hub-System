using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.FirstName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.LastName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Email)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(e => e.HireDate)
                   .IsRequired();

            builder.Property(e => e.Salary)
                   .HasColumnType("decimal(18,2)");

            builder.Property(e => e.Gender)
                   .HasMaxLength(20);

            builder.Property(e => e.PhoneNumber)
                   .HasMaxLength(30);

            builder.Property(e => e.ImageUrl)
                   .HasMaxLength(300);

            // Relationships
            builder.HasOne(e => e.Department)
                   .WithMany(d => d.Employees)
                   .HasForeignKey(e => e.DepartmentId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(e => e.Address)
                   .WithOne(a => a.Employee)
                   .HasForeignKey<EmployeeAddress>(a => a.EmployeeId)
                   .OnDelete(DeleteBehavior.Cascade); //delete address when employee is deleted
        }
    }
}



