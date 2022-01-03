using System;
using System.Collections.Generic;
using System.Linq;

namespace NGroot
{
    public class InitialDataSettings<TIdentifier>
        where TIdentifier : Enum
    {
        public string? InitialDataFolderRelativePath { get; set; }
        public List<BaseDataSettings<TIdentifier>> PathConfiguration { get; set; } = new List<BaseDataSettings<TIdentifier>>();
        public bool SeedTestData { get; set; }

        public virtual string GetLoaderFilePath(TIdentifier identifier)
            => PathConfiguration.FirstOrDefault(c => c.Identifier != null && c.Identifier.Equals(identifier))?.RelativePath ?? "";
    }

    public class BaseDataSettings<TIdentifier>
        where TIdentifier : Enum
    {
        public TIdentifier? Identifier { get; set; }
        public string? RelativePath { get; set; }
    }
}