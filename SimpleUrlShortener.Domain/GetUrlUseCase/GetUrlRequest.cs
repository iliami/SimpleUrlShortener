using MediatR;
using SimpleUrlShortener.Domain.Shared;

namespace SimpleUrlShortener.Domain.GetUrlUseCase;

public record GetUrlRequest(string UrlCode) : IRequest<Result<GetUrlResponse>>;