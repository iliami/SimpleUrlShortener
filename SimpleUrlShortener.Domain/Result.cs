using System.Text;
using FluentValidation.Results;

namespace SimpleUrlShortener.Domain;

public class Result
{ 
    public Error? Error { get; init; }

    protected Result(Error error)
    {
        Error = error;
    }

    public static Result Failure(Error error) => new(error);
}

public class Result<T> : Result
{
    private Result(Error error) : base(error) { }
    private Result(T value) : base(null!) { Value = value; }

    public T? Value { get; init; }
    
    public bool IsSuccess => Error == null;
    public bool IsFailure => !IsSuccess;
    
    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(Error error) => new(error);
    
    public static implicit operator Result<T>(T value) => Success(value);
    
    public TResult Match<TResult>(Func<Error, TResult> failure, Func<T, TResult> success)
    {
        return IsSuccess 
            ? success(Value ?? throw new NullReferenceException()) 
            : failure(Error ?? throw new NullReferenceException());
    }
}

public record Error(string Code, string? Description = null)
{
    public static readonly Error None = new Error(string.Empty);
    public static class CommonCodes
    {
        public const string Validation = "validation";
    }
}

public static class ValidationResultExtensions
{
    public static Result<T> AsResult<T>(this ValidationResult validationResult)
    {
        var sb = new StringBuilder();
        foreach (var err in validationResult.Errors)
        {
            sb.Append(err);
            sb.AppendLine();
        }
        var error = new Error(Error.CommonCodes.Validation, sb.ToString());
        return Result<T>.Failure(error);
    }
}