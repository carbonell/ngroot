
namespace NGroot
{
    public class BatchOperationResult<TModel>
    where TModel : class
    {
        public BatchOperationResult()
        {
            OperationResults = new List<DataLoadResult<TModel>>();
        }

        public ICollection<DataLoadResult<TModel>> OperationResults { get; private set; }
        public IEnumerable<TModel?> Payloads
        {
            get => OperationResults.Where(o => o.Payload != null).Select(o => o.Payload);
        }
        public IEnumerable<string> Errors
        {
            get => OperationResults.SelectMany(o => o.Errors);
        }

        public BatchOperationResult<TModel> Add(DataLoadResult<TModel> opResult)
        {
            OperationResults.Add(opResult);
            return this;
        }

        public bool AnySucceeded { get { return OperationResults.Any(o => o.Succeeded); } }
        public bool AllSucceeded { get { return OperationResults.All(o => o.Succeeded); } }
    }
}
