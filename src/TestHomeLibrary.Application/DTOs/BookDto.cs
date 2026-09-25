namespace TestHomeLibrary.Application.DTOs;

public sealed record BookDto(
    int Id,
    string Title,
    string Author,
    int PublicationYear,
    string? Publisher,
    string? Isbn,
    int? PageCount,
    string? Genre,
    string? Notes,
    string? TableOfContentsXml,
    string? TableOfContentsHtml,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
