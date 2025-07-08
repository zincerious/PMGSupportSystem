namespace PMGSupport.ThangTQ.Services.DTOs;

public class ResultDTO<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public int StatusCode { get; set; }

    public static ResultDTO<T> Ok(T data,int statusCode,string? message = null)
    {
        return new ResultDTO<T> { Success = true, Data = data, Message = message,  StatusCode = statusCode};
    }

    public static ResultDTO<T> Fail(string message, int statusCode)
    {
        return new ResultDTO<T> { Success = false, Message = message, StatusCode = statusCode };
    }
}