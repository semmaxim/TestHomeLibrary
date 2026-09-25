using Microsoft.AspNetCore.Mvc;
using TestHomeLibrary.Application.Interfaces;

namespace TestHomeLibrary.Web.Controllers;

public sealed class HomeController : Controller
{
    private readonly IBookService _bookService;

    public HomeController(IBookService bookService)
    {
        _bookService = bookService;
    }

    public IActionResult Index() => RedirectToAction(nameof(BooksController.Index), "Books");

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
