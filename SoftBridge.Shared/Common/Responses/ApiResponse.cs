namespace SoftBridge.Shared.Common.Responses
{
    public class ApiResponse<TData>
    {
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public TData? Data { get; set; }
        public List<string>? Errors { get; set; } = new List<string>();

        // Success response constructor
        // no error list needed for success responses, so it's not included here
        public ApiResponse(TData? data, string message, int statusCode = 200)
        {
            IsSuccess = true;
            Message = message;
            Data = data;
            StatusCode = statusCode;
        }
        // Failure response constructor
        // error list is optional for cases where you just want to return a message without specific errors
        public ApiResponse( string message, int statusCode = 400 , List<string>? errors = null)
        {
            IsSuccess = false;
            Message = message;
            StatusCode = statusCode;
            Errors = errors;
        }

    }
}
