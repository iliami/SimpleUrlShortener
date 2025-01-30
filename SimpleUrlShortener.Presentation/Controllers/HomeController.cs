using System.Diagnostics;
using System.Runtime.CompilerServices;
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
    public async Task<IActionResult> Index(
        [FromQuery, AsParameters] CreateUrlRequest request,
        [FromServices] IMediator mediator, 
        CancellationToken ct)
    {
        if (request.Url is null or "")
        {
            return View("Index");
        }
        var result = await mediator.Send(request, ct);
        return result.Match(
            err =>
            {
                _logger.LogError("Error {Error} occured at {EndpointName}", nameof(Index), err);
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            },
            _ => View("Index"));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}