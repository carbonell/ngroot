
namespace NGroot
{
    public class BatchOperationResult<T>
    {
        public ICollection<DataLoadResult<T>> OperationResults { get; set; } = new List<DataLoadResult<T>>();
        public IEnumerable<T?> Payloads { get { return OperationResults.Where(o => o.Payload != null).Select(o => o.Payload); } }

        public BatchOperationResult<T> Add(DataLoadResult<T> opResult)
        {
            OperationResults.Add(opResult);
            return this;
        }
    }
}
