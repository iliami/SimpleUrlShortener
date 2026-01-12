using FluentValidation;

namespace SimpleUrlShortener.Analytics.Domain.UseCases.GetAll;

public class GetAllRequestValidator : AbstractValidator<GetAllRequest>
{
    public GetAllRequestValidator()
    {
        RuleFor(r => r.Search)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .MaximumLength(2048);
    }
}