using System.ComponentModel.DataAnnotations;
using TestHomeLibrary.Application.DTOs;
using TestHomeLibrary.Application.Models;
using TestHomeLibrary.Domain.ValueObjects;
using Book = TestHomeLibrary.Domain.Entities.Book;

namespace TestHomeLibrary.Web.Models;

public sealed class BookListViewModel
{
    public IReadOnlyList<BookListItemDto> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public string? Query { get; set; }

    public string Sort { get; set; } = "title";

    public string Dir { get; set; } = "asc";

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < TotalPages;

    public bool IsSearch => !string.IsNullOrWhiteSpace(Query);

    public bool IsSortActive(string field) => !IsSearch && string.Equals(Sort, field, StringComparison.OrdinalIgnoreCase);

    public string NextDirection(string field) =>
        IsSortActive(field) && Dir == "asc" ? "desc" : "asc";
}

public sealed class BookEditViewModel
{
    public int Id { get; set; }

    [Display(Name = "Название")]
    [Required(ErrorMessage = "Укажите название.")]
    [StringLength(Book.TitleMaxLength, ErrorMessage = "Название не может быть длиннее {1} символов.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Автор")]
    [Required(ErrorMessage = "Укажите автора.")]
    [StringLength(Book.AuthorMaxLength, ErrorMessage = "Имя автора не может быть длиннее {1} символов.")]
    public string Author { get; set; } = string.Empty;

    [Display(Name = "Год издания")]
    [Range(BookYear.MinValue, BookYear.MaxValue, ErrorMessage = "Год издания должен быть в диапазоне от {1} до {2}.")]
    public int PublicationYear { get; set; }

    [Display(Name = "Издательство")]
    [StringLength(Book.PublisherMaxLength, ErrorMessage = "Название издательства не может быть длиннее {1} символов.")]
    public string? Publisher { get; set; }

    [Display(Name = "ISBN")]
    [StringLength(Book.IsbnMaxLength, ErrorMessage = "ISBN не может быть длиннее {1} символов.")]
    public string? Isbn { get; set; }

    [Display(Name = "Количество страниц")]
    [Range(1, int.MaxValue, ErrorMessage = "Количество страниц должно быть больше нуля.")]
    public int? PageCount { get; set; }

    [Display(Name = "Жанр")]
    [StringLength(Book.GenreMaxLength, ErrorMessage = "Жанр не может быть длиннее {1} символов.")]
    public string? Genre { get; set; }

    [Display(Name = "Заметки")]
    public string? Notes { get; set; }

    [Display(Name = "Оглавление")]
    public string? TableOfContentsHtml { get; set; }

    public BookEditModel ToEditModel() => new()
    {
        Id = Id,
        Title = Title,
        Author = Author,
        PublicationYear = PublicationYear,
        Publisher = Publisher,
        Isbn = Isbn,
        PageCount = PageCount,
        Genre = Genre,
        Notes = Notes,
        TableOfContentsHtml = TableOfContentsHtml
    };

    public static BookEditViewModel FromDto(BookDto dto) => new()
    {
        Id = dto.Id,
        Title = dto.Title,
        Author = dto.Author,
        PublicationYear = dto.PublicationYear,
        Publisher = dto.Publisher,
        Isbn = dto.Isbn,
        PageCount = dto.PageCount,
        Genre = dto.Genre,
        Notes = dto.Notes,
        TableOfContentsHtml = dto.TableOfContentsHtml ?? string.Empty
    };
}
