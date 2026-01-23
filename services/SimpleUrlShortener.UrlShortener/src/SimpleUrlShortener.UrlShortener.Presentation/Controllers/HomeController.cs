using Mediator;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.UrlShortener.Domain.Core;
using SimpleUrlShortener.UrlShortener.Domain.Application.UseCases;
using SimpleUrlShortener.UrlShortener.Presentation.Models;

namespace SimpleUrlShortener.UrlShortener.Presentation.Controllers;

public class HomeController(IMediator mediator, ILogger<HomeController> logger) : Controller
{
    public IActionResult Index() => View();

    [HttpGet("/")]
    public async Task<IActionResult> CreateCode(
        [FromQuery] string u,
        CancellationToken ct)
    {
        if (u is null or "")
        {
            return View("Index");
        }

        try
        {
            var request = new CreateShortUrlRequest(new OriginalUrl(u));
            var response = await mediator.Send(request, ct);
            var httpContextRequest = HttpContext.Request;
            var host = httpContextRequest.Host.Value;
            if (host is null)
            {
                return View("Error500");
            }
            var scheme = httpContextRequest.Scheme;
            var shortUrl = $"{scheme}://{host}/{response.CodePrefix}{response.Code.Value}";
            
            return View("Result",
                new UrlViewModel { OriginalUrl = request.Original.Value, ShortenedUrl = shortUrl });
        }
        catch (Exception ex)
        {
            logger.LogError("Exception at CreateCode {Exception}", ex);
            return View("Error400");
        }
    }

    [HttpGet("/{urlCode:required}")]
    public async Task<IActionResult> Index(
        string urlCode,
        CancellationToken ct)
    {
        try
        {
            var request = new GetOriginalUrlRequest(new UrlCode(urlCode[1..]));
            var response = await mediator.Send(request, ct);
            HttpContext.Response.Redirect(response.Original.Value);
            return Redirect(response.Original.Value);
        }
        catch (Exception ex)
        {
            logger.LogError("Exception at Index {Exception}", ex);
            return View("Error400");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("/error")]
    public IActionResult Error404() => View();
}