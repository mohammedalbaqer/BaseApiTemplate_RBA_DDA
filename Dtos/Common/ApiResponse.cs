namespace MyIdentityApi.Dtos.Common;

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
    public T Result { get; set; }

    public static ApiResponse<T> Success(T data, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            IsSuccess = true,
            Result = data
        };
    }

    public static ApiResponse<T> Fail(List<string> errors, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            IsSuccess = false,
            ErrorMessages = errors
        };
    }
}