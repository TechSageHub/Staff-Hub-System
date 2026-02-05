namespace Application.Dtos;

public class AnnouncementDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime DatePosted { get; set; }
    public string AuthorName { get; set; } = default!;
    public bool IsPinned { get; set; }
}

public class CreateAnnouncementDto
{
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? AuthorId { get; set; }
    public bool IsPinned { get; set; } = false;
}
