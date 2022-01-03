using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NGroot
{
    public class OperationResult
    {
        public OperationResult() { }

        public OperationResult(int? requestPoolId, int code, bool succeeded, List<string> errors)
        {
            RequestPoolId = requestPoolId;
            Code = code;
            Succeeded = succeeded;
            Errors = errors;
        }

        public OperationResult(bool succeeded, List<string> errors, int code = (int)HttpStatusCode.BadRequest)
        {
            Succeeded = succeeded;
            AddErrors(errors);
            Code = code;
        }

        public int? RequestPoolId { get; set; } // if it had a log
        public int Code { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public bool Succeeded { get; set; }

        public bool IsSuccessfulWithNoErrors { get => Succeeded && !HasErrors; }

        public List<string> Errors { get; set; } = new List<string>();

        public OperationResult AddError(string message)
        {
            Errors.Add(message);
            return this;
        }

        public bool HasErrors { get { return Errors.Any(); } }

        public OperationResult AddErrors(ICollection<string> messages)
            => messages?.Any() == true ? AddErrors(messages.ToArray()) : this;

        public OperationResult AddErrors(OperationResult operationResult)
            => operationResult?.Errors?.Any() == true ? AddErrors(operationResult.Errors) : this;

        public OperationResult AddErrors(params string[] messages)
        {
            if (Errors == null) Errors = new List<string>();
            if (messages != null) Errors.AddRange(messages.Where(c => !string.IsNullOrEmpty(c)));
            return this;
        }

        public OperationResult SetCode(int code)
        {
            this.Code = code;
            return this;
        }

        public OperationResult SetStatusCode(HttpStatusCode statusCode)
        {
            this.StatusCode = statusCode;
            return this;
        }

        public OperationResult SetFrom(OperationResult result)
        {
            this.RequestPoolId = result.RequestPoolId;
            this.Code = result.Code;

            this.Errors = this.Errors ?? new List<string>();
            if (result.Errors?.Any() == true)
                this.Errors.AddRange(result.Errors);

            return this;
        }

        public static OperationResult Success(int? requestPoolId = null)
        {
            var result = new OperationResult();
            result.Succeeded = true;
            result.RequestPoolId = requestPoolId;
            result.Code = (int)HttpStatusCode.OK;
            result.StatusCode = HttpStatusCode.OK;
            return result;
        }

        public static OperationResult Failed(ICollection<string> errors) => Failed(errors.ToArray());

        public static OperationResult Failed(params string[] errors)
        {
            var result = new OperationResult();
            result.Succeeded = false;
            result.AddErrors(errors);
            result.Code = (int)HttpStatusCode.BadRequest;
            result.StatusCode = HttpStatusCode.BadRequest;
            return result;
        }
    }

    public class OperationResult<T> : OperationResult
    {
        public T? Payload { get; set; }

        protected OperationResult() { }

        public new static OperationResult<T> Failed(ICollection<string> errors) => Failed(errors.ToArray());

        public new static OperationResult<T> Failed(params string[] errors)
        {
            var result = new OperationResult<T>();
            result.Succeeded = false;
            if (errors?.Any() == true) result.AddErrors(errors);
            return result;
        }

        public static OperationResult<T> Success(T payload, int? requestPoolId = null)
        {
            var result = new OperationResult<T>();
            result.Succeeded = true;
            result.Payload = payload;
            result.RequestPoolId = requestPoolId;
            return result;
        }

        public new static OperationResult<T> Success(int? requestPoolId = null)
        {
            var result = new OperationResult<T>();
            result.Succeeded = true;
            result.RequestPoolId = requestPoolId;
            return result;
        }

        public new OperationResult<T> AddError(string message)
        {
            base.AddError(message);
            return this;
        }

        public new OperationResult<T> AddErrors(ICollection<string> messages) => this.AddErrors(messages.ToArray());

        public OperationResult<T> AddErrors<U>(BatchOperationResult<U> operationResult) => this.AddErrors(operationResult.Errors.ToArray());

        public new OperationResult<T> AddErrors(params string[] messages)
        {
            base.AddErrors(messages);
            return this;
        }

        public new OperationResult<T> AddErrors(OperationResult operationResult) => AddErrors(operationResult.Errors);

        public new OperationResult<T> SetCode(int code)
        {
            this.Code = code;
            return this;
        }

        public new OperationResult<T> SetStatusCode(HttpStatusCode statusCode)
        {
            this.StatusCode = statusCode;
            return this;
        }

        public OperationResult<T> SetFrom<TResult>(TResult result) where TResult : OperationResult
        {
            this.RequestPoolId = result.RequestPoolId;
            this.Code = result.Code;

            this.Errors = this.Errors ?? new List<string>();
            if (result.Errors?.Any() == true)
                this.Errors.AddRange(result.Errors);

            return this;
        }

        public OperationResult<T> SetSucceeded(T payload)
        {
            this.Succeeded = true;
            this.Payload = payload;
            return this;
        }
    }
}