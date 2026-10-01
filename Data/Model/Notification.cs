using System.ComponentModel.DataAnnotations;

namespace Data.Model;

public class Notification
{
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = default!;

    public NotificationType Type { get; set; }

    [Required]
    [MaxLength(180)]
    public string Title { get; set; } = default!;

    [Required]
    [MaxLength(2000)]
    public string Body { get; set; } = default!;

    [MaxLength(500)]
    public string? LinkUrl { get; set; }

    public bool IsRead { get; set; }

    public DateTime? DigestedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
