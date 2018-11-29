using Models;

namespace LoadGIS
{
    public class LoadedData
    {
        public RouteFeature StartFeature { get; set; }
        public RouteFeature EndFeature { get; set; }
        public RouteFeature[] IntermediateFeatures { get; set; }
        public RouteFeature[] AllFeatures { get; set; }
    }
}