
namespace HttpSample.Common.Models
{
    public class Result<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T Value { get; set; }

        public bool IsSuccessStatusCode => StatusCode >= 200 && StatusCode <= 299;

        public static Result<T> Ok(T value)
        {
            return new Result<T>()
            {
                StatusCode = 200,
                Message = "Ok",
                Value = value
            };
        }

        public static Result<T> Error(int statusCode, string message)
        {
            return new Result<T>()
            {
                StatusCode = statusCode,
                Message = message,
                Value = default
            };
        }
    }
}
