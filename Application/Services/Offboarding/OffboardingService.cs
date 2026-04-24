using Application.Dtos;
using Application.Dtos.Paging;
using Data.Context;
using Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Offboarding;

public class OffboardingService : IOffboardingService
{
    private readonly EmployeeAppDbContext _context;

    private static readonly List<ChecklistItemDefinition> ItemDefinitions =
    [
        new("exit_interview", "Exit Interview", 1, true),
        new("asset_return", "Asset Return (laptop, badge, devices)", 2, true),
        new("knowledge_transfer", "Knowledge Transfer & Handover", 3, true),
        new("final_payroll", "Final Payroll & Benefits", 4, true),
        new("access_revocation", "Access Revocation (systems, email)", 5, true)
    ];

    public OffboardingService(EmployeeAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<OffboardingListItemDto>> GetListAsync(OffboardingQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);

        var q = _context.EmployeeOffboardings
            .AsNoTracking()
            .Include(o => o.Employee).ThenInclude(e => e!.Department)
            .Include(o => o.ProgressEntries).ThenInclude(p => p.OffboardingChecklistItem)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            !query.Status.Equals("all", StringComparison.OrdinalIgnoreCase) &&
            Enum.TryParse<OffboardingStatus>(query.Status, true, out var status))
        {
            q = q.Where(o => o.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(o =>
                EF.Functions.Like(o.Employee.FirstName + " " + o.Employee.LastName, $"%{s}%") ||
                EF.Functions.Like(o.Reason, $"%{s}%"));
        }

        var total = await q.CountAsync();

        var rows = await q
            .OrderByDescending(o => o.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = rows.Select(o => new OffboardingListItemDto
        {
            Id = o.Id,
            EmployeeId = o.EmployeeId,
            EmployeeName = $"{o.Employee.FirstName} {o.Employee.LastName}",
            DepartmentName = o.Employee.Department?.Name,
            Status = o.Status.ToString(),
            LastWorkingDay = o.LastWorkingDay,
            StartedAt = o.StartedAt,
            CompletedAt = o.CompletedAt,
            Reason = o.Reason,
            ProgressPercent = CalculatePercent(o.ProgressEntries)
        }).ToList();

        return new PagedResult<OffboardingListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<OffboardingSnapshotDto?> GetSnapshotAsync(Guid employeeId)
    {
        var offboarding = await LoadAsync(employeeId);
        if (offboarding == null) return null;
        return await BuildSnapshotAsync(offboarding);
    }

    public async Task<OffboardingSnapshotDto> StartAsync(StartOffboardingDto dto, string? initiatorUserId)
    {
        var employee = await _context.Employees
            .Include(e => e.Offboarding)
            .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId)
            ?? throw new InvalidOperationException("Employee not found.");

        if (employee.Offboarding != null)
        {
            throw new InvalidOperationException("This employee already has an offboarding in progress or completed.");
        }

        await EnsureCatalogAsync();
        var catalog = await _context.OffboardingChecklistItems.OrderBy(c => c.SortOrder).ToListAsync();

        var offboarding = new EmployeeOffboarding
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            LastWorkingDay = dto.LastWorkingDay.Date,
            Reason = dto.Reason.Trim(),
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            Status = OffboardingStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            StartedByUserId = initiatorUserId
        };

        foreach (var item in catalog)
        {
            offboarding.ProgressEntries.Add(new EmployeeOffboardingProgress
            {
                Id = Guid.NewGuid(),
                EmployeeOffboardingId = offboarding.Id,
                OffboardingChecklistItemId = item.Id,
                IsCompleted = false
            });
        }

        _context.EmployeeOffboardings.Add(offboarding);
        await _context.SaveChangesAsync();

        return await GetSnapshotAsync(employee.Id)
            ?? throw new InvalidOperationException("Offboarding could not be loaded after creation.");
    }

    public async Task<OffboardingSnapshotDto> UpdateItemAsync(UpdateOffboardingItemDto dto, string? actorUserId)
    {
        var offboarding = await LoadAsync(dto.EmployeeId)
            ?? throw new InvalidOperationException("No offboarding exists for this employee.");

        if (offboarding.Status != OffboardingStatus.InProgress)
        {
            throw new InvalidOperationException("Only in-progress offboardings can be edited.");
        }

        var entry = offboarding.ProgressEntries
            .FirstOrDefault(p => p.OffboardingChecklistItem.Key == dto.ItemKey)
            ?? throw new InvalidOperationException($"Checklist item '{dto.ItemKey}' not found.");

        entry.IsCompleted = dto.IsCompleted;
        entry.CompletedAt = dto.IsCompleted ? DateTime.UtcNow : null;
        entry.CompletedByUserId = dto.IsCompleted ? actorUserId : null;
        entry.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

        await _context.SaveChangesAsync();

        return await BuildSnapshotAsync(offboarding);
    }

    public async Task<OffboardingSnapshotDto> CompleteAsync(Guid employeeId, string? actorUserId)
    {
        var offboarding = await LoadAsync(employeeId)
            ?? throw new InvalidOperationException("No offboarding exists for this employee.");

        if (offboarding.Status == OffboardingStatus.Completed)
        {
            return await BuildSnapshotAsync(offboarding);
        }

        if (offboarding.Status == OffboardingStatus.Cancelled)
        {
            throw new InvalidOperationException("This offboarding was cancelled and cannot be completed.");
        }

        var allRequiredDone = offboarding.ProgressEntries
            .Where(p => p.OffboardingChecklistItem.IsRequired)
            .All(p => p.IsCompleted);

        if (!allRequiredDone)
        {
            throw new InvalidOperationException("All required checklist items must be completed first.");
        }

        offboarding.Status = OffboardingStatus.Completed;
        offboarding.CompletedAt = DateTime.UtcNow;

        var employee = await _context.Employees.FirstAsync(e => e.Id == employeeId);
        employee.IsOffboarded = true;
        employee.OffboardedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await BuildSnapshotAsync(offboarding);
    }

    public async Task<OffboardingSnapshotDto> CancelAsync(Guid employeeId, string? reason)
    {
        var offboarding = await LoadAsync(employeeId)
            ?? throw new InvalidOperationException("No offboarding exists for this employee.");

        if (offboarding.Status != OffboardingStatus.InProgress)
        {
            throw new InvalidOperationException("Only in-progress offboardings can be cancelled.");
        }

        offboarding.Status = OffboardingStatus.Cancelled;
        offboarding.CancelledAt = DateTime.UtcNow;
        offboarding.CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

        await _context.SaveChangesAsync();

        return await BuildSnapshotAsync(offboarding);
    }

    private async Task<EmployeeOffboarding?> LoadAsync(Guid employeeId)
    {
        return await _context.EmployeeOffboardings
            .Include(o => o.Employee).ThenInclude(e => e!.Department)
            .Include(o => o.ProgressEntries).ThenInclude(p => p.OffboardingChecklistItem)
            .FirstOrDefaultAsync(o => o.EmployeeId == employeeId);
    }

    private async Task<OffboardingSnapshotDto> BuildSnapshotAsync(EmployeeOffboarding offboarding)
    {
        await EnsureCatalogAsync();

        var items = offboarding.ProgressEntries
            .OrderBy(p => p.OffboardingChecklistItem.SortOrder)
            .Select(p => new OffboardingChecklistStatusDto
            {
                Key = p.OffboardingChecklistItem.Key,
                Name = p.OffboardingChecklistItem.Name,
                IsRequired = p.OffboardingChecklistItem.IsRequired,
                IsCompleted = p.IsCompleted,
                CompletedAt = p.CompletedAt,
                Notes = p.Notes
            }).ToList();

        var percent = CalculatePercent(offboarding.ProgressEntries);
        var canComplete = offboarding.Status == OffboardingStatus.InProgress &&
                          offboarding.ProgressEntries
                              .Where(p => p.OffboardingChecklistItem.IsRequired)
                              .All(p => p.IsCompleted);

        return new OffboardingSnapshotDto
        {
            Id = offboarding.Id,
            EmployeeId = offboarding.EmployeeId,
            EmployeeName = $"{offboarding.Employee.FirstName} {offboarding.Employee.LastName}",
            DepartmentName = offboarding.Employee.Department?.Name,
            Status = offboarding.Status.ToString(),
            LastWorkingDay = offboarding.LastWorkingDay,
            Reason = offboarding.Reason,
            Notes = offboarding.Notes,
            StartedAt = offboarding.StartedAt,
            CompletedAt = offboarding.CompletedAt,
            ProgressPercent = percent,
            CanComplete = canComplete,
            Items = items
        };
    }

    private static int CalculatePercent(ICollection<EmployeeOffboardingProgress> entries)
    {
        var required = entries.Where(p => p.OffboardingChecklistItem.IsRequired).ToList();
        if (required.Count == 0) return 100;
        var done = required.Count(p => p.IsCompleted);
        return (int)Math.Round(done * 100.0 / required.Count, MidpointRounding.AwayFromZero);
    }

    private async Task EnsureCatalogAsync()
    {
        var existing = await _context.OffboardingChecklistItems.Select(m => m.Key).ToListAsync();
        var missing = ItemDefinitions
            .Where(def => !existing.Contains(def.Key))
            .Select(def => new OffboardingChecklistItem
            {
                Id = Guid.NewGuid(),
                Key = def.Key,
                Name = def.Name,
                SortOrder = def.SortOrder,
                IsRequired = def.IsRequired
            })
            .ToList();

        if (missing.Count == 0) return;

        _context.OffboardingChecklistItems.AddRange(missing);
        await _context.SaveChangesAsync();
    }

    private sealed record ChecklistItemDefinition(string Key, string Name, int SortOrder, bool IsRequired);
}
