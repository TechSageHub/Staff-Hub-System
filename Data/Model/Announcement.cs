using System.ComponentModel.DataAnnotations;

namespace Data.Model;

public class Announcement
{
    public Guid Id { get; set; }
    
    [Required]
    public string Title { get; set; } = default!;

    [Required]
    public string Content { get; set; } = default!;

    public DateTime DatePosted { get; set; } = DateTime.Now;

    public string AuthorId { get; set; } = default!;

    public bool IsPinned { get; set; } = false;
}
