using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.Presentation.Models;

namespace SimpleUrlShortener.Presentation.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/")]
    public IActionResult Index()
    {
        _logger.LogInformation("Index");
        return View();
    }

    [HttpGet("/{url:required}")]
    public IActionResult ShortenUrl(string url)
    {
        _logger.LogInformation("{Url}", url);
        return View("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}