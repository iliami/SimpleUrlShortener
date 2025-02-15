using MediatR;
using SimpleUrlShortener.UrlShortener.Domain.Shared;

namespace SimpleUrlShortener.UrlShortener.Domain.CreateUrlUseCase;

public record CreateUrlRequest(string Url) : IRequest<Result<CreateUrlResponse>>;