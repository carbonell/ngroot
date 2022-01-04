
namespace NGroot
{
    public class DataLoadResult
    {

        public bool Succeeded { get { return !HasErrors; } }

        public List<string> Errors { get; set; } = new List<string>();


        public bool HasErrors { get { return Errors.Any(); } }

        public DataLoadResult AddErrors(params string[] errors)
        {
            if (Errors == null) Errors = new List<string>();
            if (errors != null) Errors.AddRange(errors.Where(c => !string.IsNullOrEmpty(c)));
            return this;
        }

    }

    public class DataLoadResult<TModel> : DataLoadResult
    where TModel : class
    {
        public TModel? Payload { get; private set; }
        public DataLoadResult(TModel? payload = null, params string[] errors)
        {
            Payload = payload;
            Errors = errors.ToList();
        }

        public DataLoadResult(string error)
        {
            Errors = new List<string> { error };
        }

    }
}