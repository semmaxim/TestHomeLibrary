namespace TestHomeLibrary.Application.Models;

public sealed record BookEditModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public int PublicationYear { get; set; }

    public string? Publisher { get; set; }

    public string? Isbn { get; set; }

    public int? PageCount { get; set; }

    public string? Genre { get; set; }

    public string? Notes { get; set; }

    public string? TableOfContentsHtml { get; set; }
}
