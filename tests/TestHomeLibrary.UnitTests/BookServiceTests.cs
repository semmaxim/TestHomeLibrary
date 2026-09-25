using Moq;
using TestHomeLibrary.Application.Interfaces;
using TestHomeLibrary.Application.Models;
using TestHomeLibrary.Application.Services;
using TestHomeLibrary.Domain.Entities;
using TestHomeLibrary.Domain.Exceptions;
using TestHomeLibrary.Domain.Interfaces;

namespace TestHomeLibrary.UnitTests;

public sealed class BookServiceTests
{
    private readonly Mock<IBookRepository> _repository = new();
    private readonly Mock<ITableOfContentsConverter> _converter = new();
    private readonly BookService _service;

    public BookServiceTests()
    {
        _service = new BookService(_repository.Object, _converter.Object);
    }

    private static BookEditModel ValidModel() => new()
    {
        Title = "Clean Architecture",
        Author = "Robert C. Martin",
        PublicationYear = 2017,
        Publisher = "Prentice Hall",
        Isbn = "978-0-13-449416-6",
        PageCount = 432,
        Genre = "Программирование",
        Notes = "Заметки",
        TableOfContentsHtml = "<p>Part I</p>"
    };

    [Fact]
    public async Task CreateAsync_ConvertsHtmlToXml_AndReturnsCreatedBook()
    {
        var model = ValidModel();
        _converter.Setup(c => c.ToXml(model.TableOfContentsHtml)).Returns("<toc><p>Part I</p></toc>");
        _repository.Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>())).ReturnsAsync(7);
        _repository.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Book.Restore(
                7, model.Title, model.Author, model.PublicationYear,
                model.Publisher, model.Isbn, model.PageCount, model.Genre, model.Notes,
                "<toc><p>Part I</p></toc>", DateTime.UtcNow, null));
        _converter.Setup(c => c.ToHtml(It.IsAny<string?>())).Returns("<p>Part I</p>");

        var result = await _service.CreateAsync(model);

        Assert.Equal(7, result.Id);
        Assert.Equal("Clean Architecture", result.Title);
        Assert.Equal("<toc><p>Part I</p></toc>", result.TableOfContentsXml);
        Assert.Equal("<p>Part I</p>", result.TableOfContentsHtml);
        _repository.Verify(r => r.AddAsync(It.Is<Book>(b => b.TableOfContents!.Xml == "<toc><p>Part I</p></toc>"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidTitle_ThrowsDomainValidationException()
    {
        var model = ValidModel();
        model.Title = "  ";

        await Assert.ThrowsAsync<DomainValidationException>(() => _service.CreateAsync(model));
        _repository.Verify(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_BookNotFound_ThrowsBookNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Book?)null);

        var ex = await Assert.ThrowsAsync<BookNotFoundException>(() => _service.UpdateAsync(ValidModel() with { Id = 99 }));

        Assert.Equal(99, ex.BookId);
    }

    [Fact]
    public async Task UpdateAsync_ExistingBook_PersistsChanges()
    {
        var model = ValidModel() with { Id = 5 };
        var existing = Book.Restore(
            5, "Old", "Old Author", 1990, null, null, null, null, null, null, DateTime.UtcNow, null);

        _repository.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _converter.Setup(c => c.ToXml(It.IsAny<string?>())).Returns((string?)null);
        _repository.Setup(r => r.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _service.UpdateAsync(model);

        Assert.Equal("Clean Architecture", result.Title);
        Assert.NotNull(result.UpdatedAt);
        _repository.Verify(r => r.UpdateAsync(It.Is<Book>(b => b.Title == "Clean Architecture"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_BookNotFound_Throws()
    {
        _repository.Setup(r => r.DeleteAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<BookNotFoundException>(() => _service.DeleteAsync(3));
    }

    [Fact]
    public async Task DeleteAsync_ExistingBook_Deletes()
    {
        _repository.Setup(r => r.DeleteAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _service.DeleteAsync(3);

        _repository.Verify(r => r.DeleteAsync(3, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingBook_ReturnsDtoWithHtml()
    {
        var book = Book.Restore(
            1, "Title", "Author", 2000, null, null, null, null, null,
            "<toc><b>x</b></toc>", DateTime.UtcNow, null);
        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(book);
        _converter.Setup(c => c.ToHtml("<toc><b>x</b></toc>")).Returns("<b>x</b>");

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("<b>x</b>", result!.TableOfContentsHtml);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownBook_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync(404, It.IsAny<CancellationToken>())).ReturnsAsync((Book?)null);

        Assert.Null(await _service.GetByIdAsync(404));
    }

    [Fact]
    public async Task GetListAsync_MapsItemsAndPagination()
    {
        var books = new List<Book>
        {
            Book.Create("A", "Author A", 2000),
            Book.Create("B", "Author B", 2001)
        };
        _repository.Setup(r => r.GetListAsync(2, 10, "title", "asc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookListPage(books, 42));

        var result = await _service.GetListAsync(2, 10, "title", "asc");

        Assert.Equal(42, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.TotalPages);
        Assert.True(result.HasPrevious);
        Assert.True(result.HasNext);
        Assert.Equal(new[] { "A", "B" }, result.Items.Select(i => i.Title));
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsEmptyResult_WithoutRepositoryCall()
    {
        var result = await _service.SearchAsync("   ", 1, 10);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        _repository.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WithTerm_CallsRepository()
    {
        var book = Book.Create("Толстой", "Автор", 1869);
        _repository.Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookListPage([book], 1));

        var result = await _service.SearchAsync("  Толст  ", 1, 10);

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        _repository.Verify(r => r.SearchAsync(It.Is<string>(s => s == "Толст"), 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}
