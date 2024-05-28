namespace MUSbooking.Common
{
    public class ValidationResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Value { get; set; }
        public string? Error { get; set; }

        public static ValidationResult<T> Success(T value)
          => new ValidationResult<T> { IsSuccess = true, Value = value };

        public static ValidationResult<T> Failure(string error)
          => new ValidationResult<T> { IsSuccess = false, Error = error };
    }
}
