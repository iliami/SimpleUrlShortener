using MediatR;
using SimpleUrlShortener.Analytics.Domain.Shared;

namespace SimpleUrlShortener.Analytics.Domain.UseCases.GetAll;

public class GetAllUseCase(IGetAllStorage storage) : IRequestHandler<GetAllRequest, Result<GetAllResponse>>
{
    public async Task<Result<GetAllResponse>> Handle(GetAllRequest request, CancellationToken ct)
    {
        var urls = await storage.GetAll(request.Search, ct);
        var response = new GetAllResponse(urls);
        return response;
    }
}