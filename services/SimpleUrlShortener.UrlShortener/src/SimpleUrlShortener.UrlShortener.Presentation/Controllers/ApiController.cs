using Mediator;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.UrlShortener.Domain.Application.UseCases;
using SimpleUrlShortener.UrlShortener.Domain.Core;
using SimpleUrlShortener.UrlShortener.Presentation.Filters;

namespace SimpleUrlShortener.UrlShortener.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController(IMediator mediator) : ControllerBase
{
    [HttpDelete("{urlCode}")]
    [ApiAuthorizationFilter]
    public async Task<ActionResult> Delete(string urlCode)
    {
        var request = new DeleteUrlMappingRequest(new UrlCode(urlCode[1..]));
        await mediator.Send(request);
        return Ok();
    }
    
    [HttpGet("/health")]
    [ApiAuthorizationFilter]
    public ActionResult Get()
    {
        return Ok(new { Ok = true });
    }
}
