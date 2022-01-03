using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace NGroot
{
    public interface IFileParser
    {
        Task<List<TModel>> ParseFile<TModel>(string path) where TModel : class;
    }

    public class FileParser : IFileParser
    {
        public async Task<List<TModel>> ParseFile<TModel>(string path) where TModel : class
        {
            var modelJsonArray = await System.IO.File.ReadAllTextAsync(path);
            var modelList = JsonSerializer.Deserialize<List<TModel>>(modelJsonArray);
            return modelList ?? new List<TModel>();
        }
    }
}