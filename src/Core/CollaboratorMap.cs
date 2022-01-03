using System;
using System.Collections.Generic;

namespace NGroot
{
    public class CollaboratorMap<TModel, TDataIdentifier>
        where TModel : class
        where TDataIdentifier : Enum
    {
        public CollaboratorMap(TDataIdentifier collaboratorId, string sourceProperty, string destinationProperty, Func<Dictionary<string, object>, TModel, object?> filterCollaborator)
        {
            CollaboratorId = collaboratorId;
            SourceProperty = sourceProperty;
            DestinationProperty = destinationProperty;
            FilterCollaborator = filterCollaborator;
        }

        public TDataIdentifier CollaboratorId { get; set; }
        public string SourceProperty { get; set; }
        public string DestinationProperty { get; set; }
        public Func<Dictionary<string, object>, TModel, object?> FilterCollaborator { get; set; }
        public Action<object, TModel>? AfterMap { get; set; }
    }
}