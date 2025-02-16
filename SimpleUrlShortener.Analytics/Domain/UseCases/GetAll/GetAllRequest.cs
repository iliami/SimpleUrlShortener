using MediatR;
using SimpleUrlShortener.Analytics.Domain.Shared;

namespace SimpleUrlShortener.Analytics.Domain.UseCases.GetAll;

public record GetAllRequest(string Search = "") : IRequest<Result<GetAllResponse>>;