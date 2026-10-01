using Application.Dtos;

namespace Application.Dtos;

public class PendingDigestDto
{
    public string UserId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public List<NotificationDto> Items { get; set; } = new();
}
