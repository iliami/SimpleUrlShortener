using Mediator;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.UrlShortener.Domain.Application.UseCases;
using SimpleUrlShortener.UrlShortener.Domain.Core;

namespace SimpleUrlShortener.UrlShortener.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController(IMediator mediator) : ControllerBase
{
    [HttpDelete("{urlCode}")]
    public async Task<ActionResult> GetById(string urlCode)
    {
        var request = new DeleteUrlMappingRequest(new UrlCode(urlCode[1..]));
        await mediator.Send(request);
        return Ok();
    }
}
