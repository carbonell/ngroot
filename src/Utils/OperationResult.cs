
namespace NGroot
{
    public class DataLoadResult
    {

        public bool Succeeded { get { return !HasErrors; } }

        public List<string> Errors { get; set; } = new List<string>();


        public bool HasErrors { get { return Errors.Any(); } }

        public DataLoadResult AddErrors(params string[] messages)
        {
            if (Errors == null) Errors = new List<string>();
            if (messages != null) Errors.AddRange(messages.Where(c => !string.IsNullOrEmpty(c)));
            return this;
        }

    }

    public class DataLoadResult<T> : DataLoadResult
    {
        public T? Payload { get; set; }

        protected DataLoadResult() { }

        public static DataLoadResult<T> Failed(params string[] errors)
        {
            var result = new DataLoadResult<T>();
            if (errors?.Any() == true) result.AddErrors(errors);
            return result;
        }

        public new DataLoadResult<T> AddErrors(params string[] messages)
        {
            base.AddErrors(messages);
            return this;
        }
        public DataLoadResult<T> SetFrom<TResult>(TResult result) where TResult : DataLoadResult
        {

            this.Errors = this.Errors ?? new List<string>();
            if (result.Errors?.Any() == true)
                this.Errors.AddRange(result.Errors);

            return this;
        }
    }
}