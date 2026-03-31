// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
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

    /// <summary>
    /// Indicates whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The domain error if the operation failed; <see cref="PadTimeError.None"/> on success.
    /// </summary>
    public PadTimeError PadTimeError { get; }

    /// <summary>
    /// Creates a successful result with no value.
    /// </summary>
    public static Result Success() => new(true, PadTimeError.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="padTimeError">The domain error describing the failure.</param>
    public static Result Failure(PadTimeError padTimeError) => new(false, padTimeError);

    /// <summary>
    /// Creates a successful result carrying a value.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="value">The value to return on success.</param>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, PadTimeError.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <typeparam name="TValue">The type of the value that would have been returned on success.</typeparam>
    /// <param name="padTimeError">The domain error describing the failure.</param>
    public static Result<TValue> Failure<TValue>(PadTimeError padTimeError) => new(default, false, padTimeError);

    /// <summary>
    /// Implicitly converts a <see cref="PadTimeError"/> to a failed <see cref="Result"/>.
    /// </summary>
    public static implicit operator Result(PadTimeError padTimeError) => Failure(padTimeError);
}

/// <summary>
/// Represents the result of an operation that can fail and returns a value of type <typeparamref name="TValue"/> on success.
/// </summary>
/// <typeparam name="TValue">The type of the value returned on success.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, PadTimeError padTimeError)
        : base(isSuccess, padTimeError)
    {
        _value = value;
    }

    /// <summary>
    /// The success value. Throws <see cref="InvalidOperationException"/> if the result is a failure.
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    /// <summary>
    /// Implicitly wraps a value in a successful result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    /// Implicitly converts a <see cref="PadTimeError"/> to a failed result.
    /// </summary>
    public static implicit operator Result<TValue>(PadTimeError padTimeError) => Failure<TValue>(padTimeError);
}