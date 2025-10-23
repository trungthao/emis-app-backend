namespace Student.Application.Common;

/// <summary>
/// Base response cho tất cả các use cases
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public List<string>? Errors { get; }

    protected Result(bool isSuccess, T? data, string? errorMessage, List<string>? errors)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        Errors = errors;
    }

    public static Result<T> Success(T data) => new(true, data, null, null);
    
    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage, null);
    
    public static Result<T> Failure(List<string> errors) => new(false, default, null, errors);
}

/// <summary>
/// Result without data
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public List<string>? Errors { get; }

    protected Result(bool isSuccess, string? errorMessage, List<string>? errors)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Errors = errors;
    }

    public static Result Success() => new(true, null, null);
    
    public static Result Failure(string errorMessage) => new(false, errorMessage, null);
    
    public static Result Failure(List<string> errors) => new(false, null, errors);
}
