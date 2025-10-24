namespace Application.Common;

public static class ResponseBuilder
{
    public static LambdaResponse<T> Ok<T>(T data)
    {
        return new LambdaResponse<T>
        {
            Success = true,
            Data = data,
            Message = "Operation completed successfully"
        };
    }

    public static LambdaResponse<T> Ok<T>(T data, string message)
    {
        return new LambdaResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static LambdaResponse<object> Error(string message)
    {
        return new LambdaResponse<object>
        {
            Success = false,
            Data = null,
            Message = message,
            Error = message
        };
    }

    public static LambdaResponse<T> Error<T>(string message)
    {
        return new LambdaResponse<T>
        {
            Success = false,
            Data = default,
            Message = message,
            Error = message
        };
    }

    public static LambdaResponse<T> Error<T>(Exception exception)
    {
        return new LambdaResponse<T>
        {
            Success = false,
            Data = default,
            Message = "An error occurred",
            Error = exception.Message
        };
    }

    public static LambdaResponse<T> Error<T>(string message, Exception exception)
    {
        return new LambdaResponse<T>
        {
            Success = false,
            Data = default,
            Message = message,
            Error = exception.Message
        };
    }
}

public class LambdaResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}