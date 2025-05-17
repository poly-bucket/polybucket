namespace Core.Models.Auth
{
    public class ServiceResponse<T>
    {
        public bool Succeeded { get; private set; }
        public string Message { get; private set; }
        public T Data { get; private set; }
        public Dictionary<string, List<string>> ValidationErrors { get; private set; }

        private ServiceResponse(bool succeeded, string message, T data, Dictionary<string, List<string>> validationErrors = null)
        {
            Succeeded = succeeded;
            Message = message;
            Data = data;
            ValidationErrors = validationErrors;
        }

        public static ServiceResponse<T> Success(T data, string message = null)
        {
            return new ServiceResponse<T>(true, message, data);
        }

        public static ServiceResponse<T> Failure(string message, T data = default)
        {
            return new ServiceResponse<T>(false, message, data);
        }

        public static ServiceResponse<T> ValidationFailure(string message, Dictionary<string, List<string>> validationErrors, T data = default)
        {
            return new ServiceResponse<T>(false, message, data, validationErrors);
        }
    }
} 