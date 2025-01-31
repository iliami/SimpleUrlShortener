﻿using FluentValidation;

namespace SimpleUrlShortener.Domain.CreateUrlUseCase;

public class CreateUrlRequestValidator : AbstractValidator<CreateUrlRequest>
{
    public CreateUrlRequestValidator()
    {
        RuleFor(r => r.Url)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Matches(@"^(https?:\/\/)?(?:www\.)?([a-zA-Z0-9\-]{2,}\.)+[a-zA-Z0-9]{2,}(?:[-a-zA-Z0-9()@:%_\+.~#?&\[\]\/=]*)$");
    }
}