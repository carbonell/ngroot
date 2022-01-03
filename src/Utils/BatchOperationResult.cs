using System.Collections.Generic;
using System.Linq;

namespace NGroot
{
    public class BatchOperationResult<T>
    {
        public ICollection<OperationResult<T>> OperationResults { get; set; } = new List<OperationResult<T>>();
        public IEnumerable<T?> Payloads { get { return OperationResults.Where(o => o.Payload != null).Select(o => o.Payload); } }

        public IEnumerable<string> Errors { get { return OperationResults.SelectMany(o => o.Errors); } }
        public BatchOperationResult<T> Add(OperationResult<T> opResult)
        {
            OperationResults.Add(opResult);
            return this;
        }

        public BatchOperationResult<T> AddFailed(OperationResult opResult)
        {
            var op = OperationResult<T>.Failed();
            op.SetFrom(opResult);
            OperationResults.Add(op);
            return this;
        }

        private int _requestPoolId;
        public int RequestPoolId
        {
            get => _requestPoolId;
            set
            {
                _requestPoolId = value;
                for (var i = 0; i < OperationResults.Count; i++)
                {
                    OperationResults.ElementAt(i).RequestPoolId = value;
                }
            }
        }

        public int Count { get { return OperationResults.Count; } }
        public bool AnySucceeded { get { return OperationResults.Any(o => o.Succeeded); } }
        public bool AllSucceeded { get { return OperationResults.All(o => o.Succeeded); } }
    }
}
