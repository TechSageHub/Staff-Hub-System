using Data.Configurations;
using Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Context;

public class EmployeeAppDbContext : IdentityDbContext<IdentityUser>
{
    public EmployeeAppDbContext(DbContextOptions<EmployeeAppDbContext> options)
        : base(options) {}

    public DbSet<Employee> Employees { get; set; } = default!;
    public DbSet<Department> Departments { get; set; } = default!;
    public DbSet<EmployeeAddress> Addresses { get; set; }
    public DbSet<NigeriaState> States { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
    public DbSet<PayrollRecord> PayrollRecords { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<AttendanceLog> AttendanceLogs { get; set; }
    public DbSet<HrTicket> HrTickets { get; set; }
    public DbSet<HrTicketComment> HrTicketComments { get; set; }
    public DbSet<PerformanceAppraisal> PerformanceAppraisals { get; set; }
    public DbSet<OnboardingModule> OnboardingModules { get; set; } = default!;
    public DbSet<EmployeeOnboardingProgress> EmployeeOnboardingProgress { get; set; } = default!;
    public DbSet<EmployeeQualification> EmployeeQualifications { get; set; } = default!;
    public DbSet<EmployeeNextOfKin> EmployeeNextOfKins { get; set; } = default!;
    public DbSet<EmployeeHrInfo> EmployeeHrInfos { get; set; } = default!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically apply all IEntityTypeConfiguration<T> from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmployeeAppDbContext).Assembly);
    }
}
