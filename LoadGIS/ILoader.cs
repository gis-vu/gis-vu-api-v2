using DTOs;
using Models;

namespace LoadGIS
{
    public interface ILoader
    {
        LoadedData Load(LoadRequest request);
    }
}