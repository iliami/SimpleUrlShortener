using System.Diagnostics;
using System.Runtime.CompilerServices;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.Domain.CreateUrlUseCase;
using SimpleUrlShortener.Domain.GetUrlUseCase;
using SimpleUrlShortener.Presentation.Models;

namespace SimpleUrlShortener.Presentation.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("/")]
    public async Task<IActionResult> CreateCode(
        [FromQuery] string u,
        [FromServices] IMediator mediator, 
        CancellationToken ct)
    {
        if (u is null or "")
        {
            return View("Index");
        }

        var request = new CreateUrlRequest(u);
        var result = await mediator.Send(request, ct);

        return result.Match(
            error =>
            {
                _logger.LogError("Error {Error} occurred at {EndpointName}", error, nameof(CreateCode));
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            },
            value =>
            {
                var httpContextRequest = HttpContext.Request;
                var host = httpContextRequest.Host.Value;
                var scheme = httpContextRequest.Scheme;
                var shortUrl = $"{scheme}://{host}/{value.Code}";
                
                return View("Result",
                    new UrlViewModel { OriginalUrl = value.OriginalUrl, ShortenedUrl = shortUrl });
            });
    }

    [HttpGet("/{urlCode:required}")]
    public async Task<IActionResult> Index(
        string urlCode,
        [FromServices] IMediator mediator,
        CancellationToken ct)
    {
        var request = new GetUrlRequest(urlCode);
        var result = await mediator.Send(request, ct);
        return result.Match<IActionResult>(
            error =>
            {
                _logger.LogError("Error {Error} occurred at {EndpointName}", error, nameof(Index));
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            },
            value =>
            {
                HttpContext.Response.Redirect(value.OriginalUrl);
                return Redirect(value.OriginalUrl)!;
            });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}