namespace OrderTracking.Domain.Common;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">A value indicating whether the operation was successful.</param>
    /// <param name="error">The error message if the operation failed.</param>
    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new Result(true, null);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    public static Result Failure(string error) => new Result(false, error);
}

/// <summary>
/// Represents the result of an operation that can succeed with a value or fail.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Gets the value if the operation was successful.
    /// </summary>
    public T? Value { get; }

    private Result(T? value, bool isSuccess, string? error) : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The value.</param>
    public static Result<T> Success(T value) => new Result<T>(value, true, null);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    public static new Result<T> Failure(string error) => new Result<T>(default, false, error);
}
