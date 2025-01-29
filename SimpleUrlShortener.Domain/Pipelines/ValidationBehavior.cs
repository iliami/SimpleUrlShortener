using System.Text;
using FluentValidation;
using MediatR;

namespace SimpleUrlShortener.Domain.Pipelines;

public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest> validator)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var validationResult = await validator.ValidateAsync(context, cancellationToken);

        if (validationResult.IsValid)
        {
            return await next();
        }
        
        var sb = new StringBuilder();
        foreach (var err in validationResult.Errors)
        {
            sb.Append(err);
            sb.AppendLine();
        }
        var error = new Error(Error.CommonCodes.Validation, sb.ToString());
        return (TResponse)Result.Failure(error);
    }
}