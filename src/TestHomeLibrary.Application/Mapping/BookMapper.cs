using TestHomeLibrary.Application.DTOs;
using TestHomeLibrary.Domain.Entities;

namespace TestHomeLibrary.Application.Mapping;

public static class BookMapper
{
    public static BookDto ToDto(Book book, string? tableOfContentsHtml = null) =>
        new(
            book.Id,
            book.Title,
            book.Author,
            book.PublicationYear.Value,
            book.Publisher,
            book.Isbn,
            book.PageCount,
            book.Genre,
            book.Notes,
            book.TableOfContents?.Xml,
            tableOfContentsHtml,
            book.CreatedAt,
            book.UpdatedAt);

    public static BookListItemDto ToListItem(Book book) =>
        new(
            book.Id,
            book.Title,
            book.Author,
            book.PublicationYear.Value,
            book.Publisher,
            book.Isbn);
}
