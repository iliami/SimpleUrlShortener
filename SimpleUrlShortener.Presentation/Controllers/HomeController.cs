using MediatR;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.Domain.CreateUrlUseCase;
using SimpleUrlShortener.Domain.GetUrlUseCase;
using SimpleUrlShortener.Domain.Shared;
using SimpleUrlShortener.Presentation.Models;

namespace SimpleUrlShortener.Presentation.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index() => View();

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
                if (error.Code is not Error.CommonCodes.Validation)
                {
                    _logger.LogError("Error {Error} occurred at {EndpointName}", error, nameof(CreateCode));
                }
                return error.Code switch
                {
                    Error.CommonCodes.Validation => View("Error400"),
                    _ => View("Error500")
                };
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
                if (error.Code is not Error.CommonCodes.Validation)
                {
                    _logger.LogError("Error {Error} occurred at {EndpointName}", error, nameof(CreateCode));
                }
                return error.Code switch
                {
                    Error.CommonCodes.Validation => View("Error400"),
                    _ => View("Error500")
                };
            },
            value =>
            {
                HttpContext.Response.Redirect(value.OriginalUrl);
                return Redirect(value.OriginalUrl)!;
            });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("/error")]
    public IActionResult Error404() => View();
}