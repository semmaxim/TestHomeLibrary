using TestHomeLibrary.Domain.Entities;
using TestHomeLibrary.Domain.Exceptions;

namespace TestHomeLibrary.UnitTests;

public sealed class BookTests
{
    [Fact]
    public void Create_ValidData_ProducesBook()
    {
        var book = Book.Create(
            "  Война и мир  ",
            " Лев Толстой ",
            1869,
            publisher: "Русский вестник",
            isbn: "978-5-17-087662-4",
            pageCount: 1225,
            genre: "Классика",
            notes: "Экземпляр из семейной библиотеки",
            tableOfContentsXml: "<toc><ol><li>Том I</li></ol></toc>");

        Assert.Equal("Война и мир", book.Title);
        Assert.Equal("Лев Толстой", book.Author);
        Assert.Equal(1869, book.PublicationYear.Value);
        Assert.Equal("Русский вестник", book.Publisher);
        Assert.Equal(1225, book.PageCount);
        Assert.NotNull(book.TableOfContents);
        Assert.Equal(0, book.Id);
        Assert.Null(book.UpdatedAt);
    }

    [Theory]
    [InlineData("", "Author")]
    [InlineData("   ", "Author")]
    [InlineData("Title", "")]
    [InlineData("Title", "   ")]
    public void Create_MissingRequiredField_Throws(string title, string author)
    {
        Assert.Throws<DomainValidationException>(() => Book.Create(title, author, 2000));
    }

    [Fact]
    public void Create_TitleTooLong_Throws()
    {
        var title = new string('а', Book.TitleMaxLength + 1);

        Assert.Throws<DomainValidationException>(() => Book.Create(title, "Author", 2000));
    }

    [Fact]
    public void Create_InvalidYear_Throws()
    {
        Assert.Throws<DomainValidationException>(() => Book.Create("Title", "Author", 1000));
    }

    [Fact]
    public void Create_NonPositivePageCount_Throws()
    {
        Assert.Throws<DomainValidationException>(() => Book.Create("Title", "Author", 2000, pageCount: 0));
    }

    [Fact]
    public void Create_InvalidTableOfContentsXml_Throws()
    {
        Assert.Throws<DomainValidationException>(() => Book.Create("Title", "Author", 2000, tableOfContentsXml: "<toc>"));
    }

    [Fact]
    public void Update_ChangesFields_AndSetsUpdatedAt()
    {
        var book = Book.Create("Old title", "Old author", 1990);
        var updatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        book.Update("New title", "New author", 2020, "Publisher", "978-0-00-000000-0", 100, "Genre", "Notes", null, updatedAt);

        Assert.Equal("New title", book.Title);
        Assert.Equal("New author", book.Author);
        Assert.Equal(2020, book.PublicationYear.Value);
        Assert.Equal(updatedAt, book.UpdatedAt);
    }

    [Fact]
    public void Restore_ReturnsBookWithPersistedState()
    {
        var created = new DateTime(2025, 5, 1, 12, 0, 0, DateTimeKind.Utc);

        var book = Book.Restore(
            42, "Title", "Author", 2000,
            publisher: null,
            isbn: null,
            pageCount: null,
            genre: null,
            notes: null,
            tableOfContentsXml: null,
            createdAt: created,
            updatedAt: null);

        Assert.Equal(42, book.Id);
        Assert.Equal(created, book.CreatedAt);
        Assert.Null(book.TableOfContents);
    }

    [Fact]
    public void Restore_InvalidId_Throws()
    {
        Assert.Throws<DomainValidationException>(() => Book.Restore(
            0, "Title", "Author", 2000, null, null, null, null, null, null, DateTime.UtcNow, null));
    }
}
