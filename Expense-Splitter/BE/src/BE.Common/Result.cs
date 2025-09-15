namespace BE.Common;

public class Result
{
    protected Result(bool succeeded, string? error = null, string? message = null)
    {
        Succeeded = succeeded;
        Error = error;
        Message = message;
    }

    public bool Succeeded { get; }
    public string? Error { get; }
    public string? Message { get; }

    public static Result Success(string? message = null) => new Result(true, null, message);
    public static Result Failure(string error) => new Result(false, error);
}

public class Result<T> : Result
{
    protected Result(bool succeeded, string? error, T? data = default, string? message = null)
        : base(succeeded, error, message)
    {
        Data = data;
    }

    public T? Data { get; }

    public static Result<T> Success(T data, string? message = null) => new Result<T>(true, null, data, message);
    public new static Result<T> Failure(string error) => new Result<T>(false, error);
}
