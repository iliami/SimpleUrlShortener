using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.Domain.CreateUrlUseCase;
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
    public async Task<IActionResult> ShortenUrl(string url, [FromServices] IMediator mediator, CancellationToken ct)
    {
        _logger.LogInformation("{Url}", url);
        var request = new CreateUrlRequest(url);
        var result = await mediator.Send(request, ct);
        return result.Match(
            err =>
            {
                _logger.LogError("{Error}", err);
                return View("Error");
            },
            val =>
            {
                _logger.LogInformation("{Value}", val);
                return View("Index");
            });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}