namespace TubieTools_PublicAPI.Models.Responses
{
    /// <summary>
    /// API Response wrapper for consistent response formatting
    /// </summary>
    /// <typeparam name="T">The data type being returned</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Response data payload
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// List of validation errors (if any)
        /// </summary>
        public List<string> Errors { get; set; }

        public ApiResponse()
        {
            Errors = new List<string>();
        }

        public ApiResponse(bool success, int statusCode, string message, T data = default)
        {
            Success = success;
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Errors = new List<string>();
        }
    }
}
