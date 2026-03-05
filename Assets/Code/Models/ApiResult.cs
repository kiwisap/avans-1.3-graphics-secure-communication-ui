namespace Assets.Code.Models
{
    public readonly struct ApiResult
    {
        public bool Ok { get; }
        public string Error { get; }

        private ApiResult(bool ok, string error)
        {
            Ok = ok;
            Error = error;
        }

        public static ApiResult Success() => new ApiResult(true, "");
        public static ApiResult Fail(string error) => new ApiResult(false, error ?? "Unknown error");
    }

    public readonly struct ApiResult<T>
    {
        public bool Ok { get; }
        public string Error { get; }
        public T Value { get; }

        private ApiResult(bool ok, string error, T value)
        {
            Ok = ok;
            Error = error;
            Value = value;
        }

        public static ApiResult<T> Success(T value) => new ApiResult<T>(true, "", value);
        public static ApiResult<T> Fail(string error) => new ApiResult<T>(false, error ?? "Unknown error", default);
    }
}