
namespace QuizzApp.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }

        // A human-readable message (e.g., "Login successful" or error description)
        public string Message { get; set; } = string.Empty;

        // The actual data returned (null on failure)
        public T? Data { get; set; }

        // Helper method to create a success response
        public static ApiResponse<T> Ok(T data, string message = "Success")
        {
            return new ApiResponse<T> { Success = true, Message = message, Data = data };
        }

        // Helper method to create a failure response
        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T> { Success = false, Message = message, Data = default };
        }

        
    }
}
