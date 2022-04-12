
namespace NGroot
{

    public class NgrootSettings
        : NgrootSettings<string>
    {

    }


    public class NgrootSettings<TIdentifier>
    {
        public string? InitialDataFolderRelativePath { get; set; }
        public List<BaseDataSettings<TIdentifier>> PathConfiguration { get; set; } = new List<BaseDataSettings<TIdentifier>>();
        public bool SeedTestData { get; set; }

        public bool LoadFromMemory { get; set; }
        public bool StopOnException { get; set; } = true;
        public virtual string GetLoaderFilePath(TIdentifier identifier)
            => PathConfiguration.FirstOrDefault(c => c.Identifier != null && c.Identifier.Equals(identifier))?.RelativePath ?? "";

    }

    public class BaseDataSettings<TIdentifier>
    {
        public TIdentifier? Identifier { get; set; }
        public string? RelativePath { get; set; }
    }
}