namespace TestHomeLibrary.Application.DTOs;

public sealed record BookListItemDto(
    int Id,
    string Title,
    string Author,
    int PublicationYear,
    string? Publisher,
    string? Isbn);
