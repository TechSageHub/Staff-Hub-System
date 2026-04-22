using System.ComponentModel.DataAnnotations;

namespace Data.Model;

public class HrTicketComment
{
    public Guid Id { get; set; }

    [Required]
    public Guid HrTicketId { get; set; }
    public HrTicket HrTicket { get; set; } = default!;

    [Required]
    [MaxLength(150)]
    public string CommenterName { get; set; } = default!;

    public bool IsAdminComment { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
