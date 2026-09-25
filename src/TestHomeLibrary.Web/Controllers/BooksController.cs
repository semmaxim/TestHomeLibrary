using Microsoft.AspNetCore.Mvc;
using TestHomeLibrary.Application.DTOs;
using TestHomeLibrary.Application.Interfaces;
using TestHomeLibrary.Domain.Exceptions;
using TestHomeLibrary.Web.Models;

namespace TestHomeLibrary.Web.Controllers;

public sealed class BooksController : Controller
{
    public const int DefaultPageSize = 10;

    private static readonly string[] AllowedSortFields = ["title", "author", "year", "publisher", "isbn"];

    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    public async Task<IActionResult> Index(
        int page = 1,
        string? q = null,
        string? sort = null,
        string? dir = null,
        CancellationToken cancellationToken = default)
    {
        var sortField = NormalizeSortField(sort);
        var sortDirection = NormalizeSortDirection(dir);

        PagedResult<BookListItemDto> result;
        if (!string.IsNullOrWhiteSpace(q))
        {
            result = await _bookService.SearchAsync(q.Trim(), page, DefaultPageSize, cancellationToken);
        }
        else
        {
            result = await _bookService.GetListAsync(page, DefaultPageSize, sortField, sortDirection, cancellationToken);
        }

        var viewModel = new BookListViewModel
        {
            Items = result.Items,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Query = q?.Trim(),
            Sort = sortField,
            Dir = sortDirection
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);
        return book is null ? NotFound() : View(book);
    }

    public IActionResult Create()
    {
        return View(new BookEditViewModel { PublicationYear = DateTime.Today.Year });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookEditViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var created = await _bookService.CreateAsync(model.ToEditModel(), cancellationToken);
            TempData["SuccessMessage"] = "Книга успешно добавлена.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (DomainValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            return NotFound();
        }

        return View(BookEditViewModel.FromDto(book));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookEditViewModel model, CancellationToken cancellationToken = default)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _bookService.UpdateAsync(model.ToEditModel(), cancellationToken);
            TempData["SuccessMessage"] = "Изменения сохранены.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
        catch (BookNotFoundException)
        {
            return NotFound();
        }
        catch (DomainValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);
        return book is null ? NotFound() : View(book);
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _bookService.DeleteAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Книга удалена.";
            return RedirectToAction(nameof(Index));
        }
        catch (BookNotFoundException)
        {
            return NotFound();
        }
    }

    private static string NormalizeSortField(string? sort) =>
        AllowedSortFields.Contains((sort ?? "title").Trim().ToLowerInvariant())
            ? (sort ?? "title").Trim().ToLowerInvariant()
            : "title";

    private static string NormalizeSortDirection(string? dir) =>
        string.Equals(dir?.Trim(), "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
}
