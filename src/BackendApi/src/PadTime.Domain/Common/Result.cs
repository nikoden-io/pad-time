namespace PadTime.Domain.Common;

/// <summary>
/// Represents the result of an operation that can fail.
/// Used to avoid throwing exceptions for expected business rule violations.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, PadTimeError padTimeError)
    {
        if (isSuccess && padTimeError != PadTimeError.None)
            throw new InvalidOperationException("Success result cannot have an error.");

        if (!isSuccess && padTimeError == PadTimeError.None)
            throw new InvalidOperationException("Failure result must have an error.");

        IsSuccess = isSuccess;
        PadTimeError = padTimeError;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public PadTimeError PadTimeError { get; }

    public static Result Success() => new(true, PadTimeError.None);
    public static Result Failure(PadTimeError padTimeError) => new(false, padTimeError);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, PadTimeError.None);
    public static Result<TValue> Failure<TValue>(PadTimeError padTimeError) => new(default, false, padTimeError);

    public static implicit operator Result(PadTimeError padTimeError) => Failure(padTimeError);
}

/// <summary>
/// Represents the result of an operation that can fail and returns a value on success.
/// </summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, PadTimeError padTimeError)
        : base(isSuccess, padTimeError)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(PadTimeError padTimeError) => Failure<TValue>(padTimeError);
}
