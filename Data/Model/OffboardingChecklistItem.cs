namespace Data.Model;

public class OffboardingChecklistItem
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public ICollection<EmployeeOffboardingProgress> ProgressEntries { get; set; } = new List<EmployeeOffboardingProgress>();
}
