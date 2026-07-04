namespace BLL.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? Error { get; }

        // Private constructor to force the use of Factory methods
        private Result(bool isSuccess, T? value, string? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }


        // Factory method for Success
        public static Result<T> Success(T value)
        {
            return new(true, value, null);
        }

        // Factory method for Failure
        public static Result<T> Failure(string error) => new(false, default, error);
    }
}