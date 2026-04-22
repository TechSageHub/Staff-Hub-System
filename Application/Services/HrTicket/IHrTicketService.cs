using Application.Dtos;
using Application.Dtos.Paging;

namespace Application.Services.HrTicket;

public interface IHrTicketService
{
    Task<HrTicketDto> CreateTicketAsync(CreateHrTicketDto dto);
    Task<List<HrTicketDto>> GetEmployeeTicketsAsync(Guid employeeId);
    Task<List<HrTicketDto>> GetAllTicketsAsync(string? status = null);
    Task<PagedResult<HrTicketDto>> GetAllTicketsPagedAsync(HrTicketQuery query);
    Task<HrTicketDto?> GetTicketByIdAsync(Guid id);
    Task<bool> UpdateTicketStatusAsync(UpdateHrTicketStatusDto dto);
    Task<bool> AddCommentAsync(AddHrTicketCommentDto dto, string commenterName, bool isAdminComment);
}
