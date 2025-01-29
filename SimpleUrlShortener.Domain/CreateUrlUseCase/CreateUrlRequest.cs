using MediatR;

namespace SimpleUrlShortener.Domain.CreateUrlUseCase;

public record CreateUrlRequest(string Url) : IRequest<Result<CreateUrlResponse>>;