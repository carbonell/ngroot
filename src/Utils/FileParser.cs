using System.Text.Json;

namespace NGroot
{
    public interface IFileLoader
    {
        Task<List<TModel>> LoadFile<TModel>(string path) where TModel : class;
    }

    public class FileParser : IFileLoader
    {
        public async Task<List<TModel>> LoadFile<TModel>(string path) where TModel : class
        {
            var modelJsonArray = await System.IO.File.ReadAllTextAsync(path);
            var modelList = JsonSerializer.Deserialize<List<TModel>>(modelJsonArray);
            return modelList ?? new List<TModel>();
        }
    }
}