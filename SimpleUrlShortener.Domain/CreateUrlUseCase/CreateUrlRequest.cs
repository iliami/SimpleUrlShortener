﻿using MediatR;
using SimpleUrlShortener.Domain.Shared;

namespace SimpleUrlShortener.Domain.CreateUrlUseCase;

public record CreateUrlRequest(string Url) : IRequest<Result<CreateUrlResponse>>;