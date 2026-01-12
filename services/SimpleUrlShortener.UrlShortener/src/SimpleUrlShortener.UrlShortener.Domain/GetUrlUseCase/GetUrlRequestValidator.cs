using System.Text.RegularExpressions;
using FluentValidation;

namespace SimpleUrlShortener.UrlShortener.Domain.GetUrlUseCase;

public class GetUrlRequestValidator : AbstractValidator<GetUrlRequest>
{
    public GetUrlRequestValidator()
    {
        var escapedAlphabet = Regex.Escape(UrlCodeSettings.Alphabet);

        var pattern = @$"^[{escapedAlphabet}]{{{UrlCodeSettings.CodeLength}}}$";
        
        RuleFor(r => r.UrlCode)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Matches(pattern);
    }
}