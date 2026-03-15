namespace StudentManagementSystem.BLL.Common;

public class ServiceResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public static ServiceResult Success() => new()
    {
        Succeeded = true
    };

    public static ServiceResult Failure(string errorMessage) => new()
    {
        Succeeded = false,
        ErrorMessage = errorMessage
    };
}

public sealed class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Success(T data) => new()
    {
        Succeeded = true,
        Data = data
    };

    public new static ServiceResult<T> Failure(string errorMessage) => new()
    {
        Succeeded = false,
        ErrorMessage = errorMessage
    };
}
