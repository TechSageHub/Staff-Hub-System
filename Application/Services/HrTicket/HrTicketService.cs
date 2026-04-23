using Application.Dtos;
using Application.Dtos.Paging;
using Application.Services.Email;
using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.HrTicket;

public class HrTicketService(
    EmployeeAppDbContext _context,
    IEmailService _emailService,
    UserManager<IdentityUser> _userManager) : IHrTicketService
{
    public async Task<HrTicketDto> CreateTicketAsync(CreateHrTicketDto dto)
    {
        if (!Enum.TryParse<HrTicketPriority>(dto.Priority, true, out var priority))
        {
            priority = HrTicketPriority.Medium;
        }

        var ticket = new Data.Model.HrTicket
        {
            Id = Guid.NewGuid(),
            EmployeeId = dto.EmployeeId,
            Category = dto.Category.Trim(),
            Subject = dto.Subject.Trim(),
            Description = dto.Description.Trim(),
            Priority = priority,
            Status = HrTicketStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _context.HrTickets.AddAsync(ticket);
        await _context.SaveChangesAsync();

        var saved = await _context.HrTickets
            .Include(t => t.Employee)
            .Include(t => t.Comments.OrderBy(c => c.CreatedAt))
            .FirstAsync(t => t.Id == ticket.Id);

        return ToDto(saved);
    }

    public async Task<List<HrTicketDto>> GetEmployeeTicketsAsync(Guid employeeId)
    {
        var tickets = await _context.HrTickets
            .Include(t => t.Employee)
            .Include(t => t.Comments.OrderBy(c => c.CreatedAt))
            .Where(t => t.EmployeeId == employeeId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(ToDto).ToList();
    }

    public async Task<List<HrTicketDto>> GetAllTicketsAsync(string? status = null)
    {
        IQueryable<Data.Model.HrTicket> query = _context.HrTickets
            .Include(t => t.Employee)
            .Include(t => t.Comments.OrderBy(c => c.CreatedAt));

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<HrTicketStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(t => t.Status == parsedStatus);
        }

        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(ToDto).ToList();
    }

    public async Task<PagedResult<HrTicketDto>> GetAllTicketsPagedAsync(HrTicketQuery q)
    {
        IQueryable<Data.Model.HrTicket> query = _context.HrTickets
            .Include(t => t.Employee)
            .ThenInclude(e => e!.Department)
            .Include(t => t.Comments.OrderBy(c => c.CreatedAt));

        if (!string.IsNullOrWhiteSpace(q.Status) && !string.Equals(q.Status, "all", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<HrTicketStatus>(q.Status, true, out var parsedStatus))
            {
                query = query.Where(t => t.Status == parsedStatus);
            }
        }

        if (q.DepartmentId.HasValue)
        {
            query = query.Where(t => t.Employee != null && t.Employee.DepartmentId == q.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var pattern = $"%{q.Search.Trim()}%";
            query = query.Where(t =>
                EF.Functions.Like(t.Subject, pattern) ||
                EF.Functions.Like(t.Category, pattern) ||
                (t.Employee != null &&
                 (EF.Functions.Like(t.Employee.FirstName, pattern) ||
                  EF.Functions.Like(t.Employee.LastName, pattern) ||
                  EF.Functions.Like(t.Employee.Email, pattern))));
        }

        query = query.OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync();
        var tickets = await query.Skip(q.Skip).Take(q.PageSize).ToListAsync();

        return new PagedResult<HrTicketDto>
        {
            Items = tickets.Select(ToDto).ToList(),
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<HrTicketDto?> GetTicketByIdAsync(Guid id)
    {
        var ticket = await _context.HrTickets
            .Include(t => t.Employee)
            .Include(t => t.Comments.OrderBy(c => c.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == id);

        return ticket == null ? null : ToDto(ticket);
    }

    public async Task<bool> UpdateTicketStatusAsync(UpdateHrTicketStatusDto dto)
    {
        var ticket = await _context.HrTickets
            .Include(t => t.Employee)
            .FirstOrDefaultAsync(t => t.Id == dto.Id);
        if (ticket == null)
        {
            return false;
        }

        if (!Enum.TryParse<HrTicketStatus>(dto.Status, true, out var parsedStatus))
        {
            return false;
        }

        ticket.Status = parsedStatus;
        ticket.AdminComment = string.IsNullOrWhiteSpace(dto.AdminComment) ? null : dto.AdminComment.Trim();
        ticket.UpdatedAt = DateTime.UtcNow;
        ticket.ResolvedAt = parsedStatus == HrTicketStatus.Resolved ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(ticket.Employee?.Email))
        {
            var subject = $"HR Ticket Update: {ticket.Subject}";
            var body = $@"
                <p>Hello {ticket.Employee.FirstName},</p>
                <p>Your HR ticket status was updated to <strong>{ticket.Status}</strong>.</p>
                <p><strong>Ticket:</strong> {ticket.Subject}</p>
                {(string.IsNullOrWhiteSpace(ticket.AdminComment) ? string.Empty : $"<p><strong>Admin Note:</strong> {ticket.AdminComment}</p>")}
                <p>Regards,<br/>HR Team</p>";
            await TrySendEmailAsync(ticket.Employee.Email, subject, body);
        }

        return true;
    }

    public async Task<bool> AddCommentAsync(AddHrTicketCommentDto dto, string commenterName, bool isAdminComment)
    {
        var ticket = await _context.HrTickets
            .Include(t => t.Employee)
            .FirstOrDefaultAsync(t => t.Id == dto.HrTicketId);
        if (ticket == null)
        {
            return false;
        }

        var comment = new HrTicketComment
        {
            Id = Guid.NewGuid(),
            HrTicketId = ticket.Id,
            CommenterName = commenterName,
            IsAdminComment = isAdminComment,
            Message = dto.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _context.HrTicketComments.AddAsync(comment);
        ticket.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var subject = $"New Comment on HR Ticket: {ticket.Subject}";
        var body = $@"
            <p>A new comment was added on ticket <strong>{ticket.Subject}</strong>.</p>
            <p><strong>By:</strong> {commenterName}</p>
            <p><strong>Comment:</strong> {comment.Message}</p>";

        if (isAdminComment)
        {
            if (!string.IsNullOrWhiteSpace(ticket.Employee?.Email))
            {
                await TrySendEmailAsync(ticket.Employee.Email, subject, body);
            }
        }
        else
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var adminEmails = admins
                .Select(a => a.Email)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Distinct()
                .ToList();

            foreach (var email in adminEmails!)
            {
                await TrySendEmailAsync(email!, subject, body);
            }
        }

        return true;
    }

    private static HrTicketDto ToDto(Data.Model.HrTicket ticket)
    {
        return new HrTicketDto
        {
            Id = ticket.Id,
            EmployeeId = ticket.EmployeeId,
            EmployeeName = ticket.Employee != null ? $"{ticket.Employee.FirstName} {ticket.Employee.LastName}" : "Unknown",
            EmployeeEmail = ticket.Employee?.Email ?? "-",
            Category = ticket.Category,
            Subject = ticket.Subject,
            Description = ticket.Description,
            Priority = ticket.Priority.ToString(),
            Status = ticket.Status.ToString(),
            AdminComment = ticket.AdminComment,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ResolvedAt = ticket.ResolvedAt,
            Comments = ticket.Comments
                .OrderBy(c => c.CreatedAt)
                .Select(c => new HrTicketCommentDto
                {
                    Id = c.Id,
                    HrTicketId = c.HrTicketId,
                    CommenterName = c.CommenterName,
                    IsAdminComment = c.IsAdminComment,
                    Message = c.Message,
                    CreatedAt = c.CreatedAt
                }).ToList()
        };
    }

    private async Task TrySendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            await _emailService.SendEmailAsync(toEmail, subject, body);
        }
        catch
        {
            // Do not fail ticket operations if SMTP is temporarily unavailable.
        }
    }
}
