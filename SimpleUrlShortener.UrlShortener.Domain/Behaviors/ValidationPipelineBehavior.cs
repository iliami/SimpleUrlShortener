using System.Reflection;
using System.Text;
using FluentValidation;
using MediatR;
using SimpleUrlShortener.UrlShortener.Domain.Shared;

namespace SimpleUrlShortener.UrlShortener.Domain.Behaviors;

public class ValidationPipelineBehavior<TRequest, TResponse>(IValidator<TRequest> validator)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            var sb = new StringBuilder();
            foreach (var err in validationResult.Errors)
            {
                sb.Append(err);
                sb.Append("; ");
            }

            var error = new Error(Error.CommonCodes.Validation, sb.ToString());

            var result = typeof(Result<>)
                .GetGenericTypeDefinition()
                .MakeGenericType(typeof(TResponse).GenericTypeArguments[0])
                .GetMethod("Failure", BindingFlags.Static | BindingFlags.Public)!
                .Invoke(null, [error])!;

            return result as TResponse ?? throw new InvalidOperationException();
        }

        return await next();
    }
}