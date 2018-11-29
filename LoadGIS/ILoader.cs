using Models;

namespace LoadGIS
{
    public interface ILoader
    {
        LoadedData Load(PointPosition pointPosition, PointPosition pointPosition1, PointPosition[] toArray);
    }
}