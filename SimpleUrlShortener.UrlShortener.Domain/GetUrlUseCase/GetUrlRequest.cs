using MediatR;
using SimpleUrlShortener.UrlShortener.Domain.Shared;

namespace SimpleUrlShortener.UrlShortener.Domain.GetUrlUseCase;

public record GetUrlRequest(string UrlCode) : IRequest<Result<GetUrlResponse>>;