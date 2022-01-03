using System.Linq.Expressions;
using Microsoft.Extensions.Options;

namespace NGroot
{
    public delegate Task<OperationResult<TModel>> CreateModel<TModel>(TModel model);
    public delegate Task<OperationResult<TModel>> OverrideDuplicate<TModel>(TModel model, TModel duplicate);
    public delegate Task<TModel> FindDuplicate<TModel>(TModel model);

    public interface IModelLoader
    {
        Task<BatchOperationResult<object>> LoadInitialData(string contentRootPath, Dictionary<string, object> collaborators);
        string Key { get; }
    }

    public abstract class ModelLoader<TModel, TDataIdentifier>
        : ModelLoader<TModel, TDataIdentifier, InitialDataSettings<TDataIdentifier>>,
        IModelLoader
        where TModel : class
        where TDataIdentifier : Enum
    {
        public ModelLoader(IFileParser fileLoader, IOptions<InitialDataSettings<TDataIdentifier>> settings)
            : base(fileLoader, settings)
        { }
    }

    public abstract class ModelLoader<TModel, TDataIdentifier, TSettings>
        : IModelLoader
        where TModel : class
        where TDataIdentifier : Enum
        where TSettings : InitialDataSettings<TDataIdentifier>, new()
    {
        protected readonly IFileParser _fileLoader;
        protected readonly TSettings _settings;
        protected CreateModel<TModel>? _createModelFunc;
        protected OverrideDuplicate<TModel>? _overrideDuplicateFunc;
        protected string _key;
        protected FindDuplicate<TModel>? _findDuplicatesFunc;
        protected string? _contentRootPath;

        protected string _fileRelPath;

        public virtual string Key { get { return _key; } }

        protected Dictionary<TDataIdentifier, CollaboratorMap<TModel, TDataIdentifier>> _mappingExpressions = new Dictionary<TDataIdentifier, CollaboratorMap<TModel, TDataIdentifier>>();


        public ModelLoader(IFileParser fileLoader, IOptions<TSettings> settings)
        {
            _fileLoader = fileLoader;
            _settings = settings.Value;
            _fileRelPath = "";
            _key = "";
        }

        public ModelLoader<TModel, TDataIdentifier, TSettings> SetupLoader(string key, string filePath)
        {
            _key = key;
            _fileRelPath = filePath;
            return this;
        }

        public ModelLoader<TModel, TDataIdentifier, TSettings> SetupLoader(TDataIdentifier model)
        {
            _key = model.ToString();
            _fileRelPath = _settings?.GetLoaderFilePath(model) ?? "";
            return this;
        }

        public ModelLoader<TModel, TDataIdentifier, TSettings> FindDuplicatesWith(FindDuplicate<TModel> findDuplicates)
        {
            _findDuplicatesFunc = findDuplicates;
            return this;
        }

        public ModelLoader<TModel, TDataIdentifier, TSettings> OverrideDuplicatesWith(OverrideDuplicate<TModel> overrideDuplicateFunc)
        {
            _overrideDuplicateFunc = overrideDuplicateFunc;
            return this;
        }

        public ModelLoader<TModel, TDataIdentifier, TSettings> CreateModelUsing(CreateModel<TModel> createModelUsing)
        {
            _createModelFunc = createModelUsing;
            return this;
        }

        public ModelLoader<TModel, TDataIdentifier, TSettings> With<TCollaborator>(TDataIdentifier collaboratorId, Expression<Func<TModel, object>> modelProperty, Expression<Func<TCollaborator, object>> collaboratorProperty, Func<TCollaborator, TModel, bool> filterExpression, Action<TCollaborator, TModel>? afterMap = null)
        where TCollaborator : class
        {
            Func<Dictionary<string, object>, TModel, object?> filterCollab = (collaborators, model) =>
            {
                return ParseCollaborator<TCollaborator>(collaborators, collaboratorId, model, filterExpression);
            };

            var modelDestination = modelProperty.Body.GetMemberName() ?? string.Empty;
            var collaboratorSource = collaboratorProperty.Body.GetMemberName() ?? string.Empty;
            var mappingExpression = new CollaboratorMap<TModel, TDataIdentifier>
            (collaboratorId, collaboratorSource, modelDestination, filterCollab);


            if (afterMap != null)
            {
                Action<object, TModel> additionalMap = (source, dest) => afterMap((TCollaborator)source, dest);
                mappingExpression.AfterMap = additionalMap;
            }
            _mappingExpressions.Add(collaboratorId, mappingExpression);
            return this;
        }

        protected virtual object? ParseCollaborator<TCollaborator>(Dictionary<string, object> collaborators, TDataIdentifier collaboratorId, TModel model, Func<TCollaborator, TModel, bool> filterExpression)
        {
            var collaboratorList = GetCollaborator<TCollaborator>(collaborators, collaboratorId);
            return collaboratorList.FirstOrDefault(model, filterExpression);

        }


        public async Task<BatchOperationResult<TModel>> ConfigureInitialData(string contentRootPath, Dictionary<string, object> collaborators)
        {
            var opResult = new BatchOperationResult<TModel>();
            _contentRootPath = contentRootPath;
            try
            {
                string path = GetFullFilePath();
                List<TModel> models = await ParseModel(path, collaborators);

                foreach (var model in models)
                {
                    TModel duplicate = await GetExistingModel(model);

                    if (duplicate == null)
                    {
                        OperationResult<TModel> result = await CreateModel(model);
                        opResult.Add(result);
                    }
                    else
                    {
                        if (_overrideDuplicateFunc != null)
                        {
                            var result = await _overrideDuplicateFunc(model, duplicate);
                            opResult.Add(result);
                        }
                        else
                        {
                            var failedResult = OperationResult<TModel>.Failed("This model was already added.");
                            failedResult.Payload = duplicate;
                            opResult.Add(failedResult);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                opResult.Add(OperationResult<TModel>.Failed(e.Message));
            }
            return opResult;
        }

        protected virtual string GetFullFilePath()
        {
            if (string.IsNullOrEmpty(_contentRootPath))
                throw new InvalidOperationException($"content root path not set for {this.GetType().Name}.");

            if (string.IsNullOrEmpty(_settings.InitialDataFolderRelativePath))
                throw new InvalidOperationException($"Initial data path not set.");

            var filePath = GetFilePathRelativeToInitialData();
            if (string.IsNullOrEmpty(filePath))
                throw new InvalidOperationException($"File path not set for {this.GetType().Name}.");

            var path = System.IO.Path.Combine(_contentRootPath, _settings.InitialDataFolderRelativePath, filePath);
            return path;
        }

        protected virtual async Task<List<TModel>> ParseModel(string filePath, Dictionary<string, object> collaborators)
        {
            var models = await _fileLoader.ParseFile<TModel>(filePath);
            return ParseCollaborators(models, collaborators);
        }

        protected virtual List<TModel> ParseCollaborators(List<TModel> models, Dictionary<string, object> collaborators)
        {
            foreach (var model in models)
            {
                foreach (var expressionPair in _mappingExpressions)
                {
                    var expression = expressionPair.Value;
                    var collaborator = expression.FilterCollaborator(collaborators, model);
                    if (collaborator != null)
                    {
                        var sourceValue = collaborator.GetPropertyValue<object>(expression.SourceProperty);
                        if (sourceValue != null)
                        {
                            model.SetPropertyValue(expression.DestinationProperty, sourceValue);
                        }
                        expression.AfterMap?.Invoke(collaborator, model);
                    }
                }
            }

            return models;
        }

        protected virtual Task<TModel> GetExistingModel(TModel model)
        {
            if (_findDuplicatesFunc == null)
                throw new InvalidOperationException($"Find duplicates function not set for {this.GetType().Name}.");
            return _findDuplicatesFunc(model);
        }

        protected virtual Task<OperationResult<TModel>> CreateModel(TModel model)
        {
            if (_createModelFunc == null)
                throw new InvalidOperationException($"Create model function not set for {this.GetType().Name}.");
            return _createModelFunc(model);
        }

        protected virtual string GetFilePathRelativeToInitialData()
            => _fileRelPath;

        public async Task<BatchOperationResult<object>> LoadInitialData(string contentRootPath, Dictionary<string, object> collaborators)
        {
            var batchDataResult = new BatchOperationResult<object>();

            var initialDataResult = await ConfigureInitialData(contentRootPath, collaborators);
            foreach (var data in initialDataResult.OperationResults)
            {
                var tempOpResult = OperationResult<object>.Failed();
                tempOpResult.SetFrom(data);
                tempOpResult.Succeeded = data.Succeeded;
                if (data.Payload != null)
                    tempOpResult.Payload = data.Payload;

                batchDataResult.Add(tempOpResult);
            }

            return batchDataResult;
        }

        protected List<TCollaborator> GetCollaborator<TCollaborator>(Dictionary<string, object> collaborators, string key)
        {
            var modelList = new List<TCollaborator>();

            collaborators.TryGetValue(key, out var content);
            if (content != null)
                modelList = ((IEnumerable<object>)content).Select(u => (TCollaborator)u).ToList();

            return modelList;
        }

        protected List<TCollaborator> GetCollaborator<TCollaborator>(Dictionary<string, object> collaborators, TDataIdentifier model)
            => GetCollaborator<TCollaborator>(collaborators, model.ToString());
    }
}