using FluentValidation;

namespace SimpleUrlShortener.UrlShortener.Domain.CreateUrlUseCase;

public class CreateUrlRequestValidator : AbstractValidator<CreateUrlRequest>
{
    public CreateUrlRequestValidator()
    {
        RuleFor(r => r.Url)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Matches(@"^(https?:\/\/)?(www\.)?([a-zA-Z0-9\-]+\.)+[a-zA-Z0-9]+([-a-zA-Z0-9()@:%_\+.~#?&\[\]\/=]*)$");
    }
}