using System;
namespace Utilities;

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }

    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && error != string.Empty)
            throw new InvalidOperationException("Successful result cannot have an error message.");

        if (!isSuccess && error == string.Empty)
            throw new InvalidOperationException("Failed result must have an error message.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);

    public void Deconstruct(out bool isSuccess, out string error)
    {
        isSuccess = IsSuccess;
        error = Error;
    }
}

public class Result<T> : Result
{
    public T Value { get; }

    protected Result(T value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, string.Empty);
    public new static Result<T> Failure(string error) => new(default!, false, error);

    public void Deconstruct(out bool isSuccess, out string error, out T value)
    {
        isSuccess = IsSuccess;
        value = Value;
        error = Error;
    }
}