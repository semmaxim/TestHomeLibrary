using TestHomeLibrary.Domain.Exceptions;
using TestHomeLibrary.Domain.ValueObjects;

namespace TestHomeLibrary.Domain.Entities;

public sealed class Book
{
    public const int TitleMaxLength = 500;
    public const int AuthorMaxLength = 300;
    public const int PublisherMaxLength = 300;
    public const int IsbnMaxLength = 20;
    public const int GenreMaxLength = 100;

    public int Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public BookYear PublicationYear { get; private set; }
    public string? Publisher { get; private set; }
    public string? Isbn { get; private set; }
    public int? PageCount { get; private set; }
    public string? Genre { get; private set; }
    public string? Notes { get; private set; }
    public TableOfContents? TableOfContents { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Book()
    {
    }

    public static Book Create(
        string title,
        string author,
        int publicationYear,
        string? publisher = null,
        string? isbn = null,
        int? pageCount = null,
        string? genre = null,
        string? notes = null,
        string? tableOfContentsXml = null)
    {
        var book = new Book { CreatedAt = DateTime.UtcNow };
        book.Apply(
            title,
            author,
            publicationYear,
            publisher,
            isbn,
            pageCount,
            genre,
            notes,
            tableOfContentsXml);
        return book;
    }

    public static Book Restore(
        int id,
        string title,
        string author,
        int publicationYear,
        string? publisher,
        string? isbn,
        int? pageCount,
        string? genre,
        string? notes,
        string? tableOfContentsXml,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        if (id <= 0)
        {
            throw new DomainValidationException("Идентификатор книги должен быть положительным числом.");
        }

        var book = new Book { Id = id, CreatedAt = createdAt, UpdatedAt = updatedAt };
        book.Apply(
            title,
            author,
            publicationYear,
            publisher,
            isbn,
            pageCount,
            genre,
            notes,
            tableOfContentsXml);
        return book;
    }

    public void Update(
        string title,
        string author,
        int publicationYear,
        string? publisher,
        string? isbn,
        int? pageCount,
        string? genre,
        string? notes,
        string? tableOfContentsXml,
        DateTime updatedAtUtc)
    {
        Apply(title, author, publicationYear, publisher, isbn, pageCount, genre, notes, tableOfContentsXml);
        UpdatedAt = updatedAtUtc;
    }

    private void Apply(
        string title,
        string author,
        int publicationYear,
        string? publisher,
        string? isbn,
        int? pageCount,
        string? genre,
        string? notes,
        string? tableOfContentsXml)
    {
        Title = NormalizeRequired(title, nameof(Title), TitleMaxLength);
        Author = NormalizeRequired(author, nameof(Author), AuthorMaxLength);
        PublicationYear = BookYear.From(publicationYear);
        Publisher = NormalizeOptional(publisher, PublisherMaxLength);
        Isbn = NormalizeOptional(isbn, IsbnMaxLength);
        Genre = NormalizeOptional(genre, GenreMaxLength);
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        if (pageCount is <= 0)
        {
            throw new DomainValidationException("Количество страниц должно быть больше нуля.");
        }

        PageCount = pageCount;
        TableOfContents = TableOfContents.FromXml(tableOfContentsXml);
    }

    private static string NormalizeRequired(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException($"Поле \"{fieldName}\" обязательно для заполнения.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainValidationException(
                $"Поле \"{fieldName}\" не может быть длиннее {maxLength} символов.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainValidationException($"Значение не может быть длиннее {maxLength} символов.");
        }

        return normalized;
    }
}
