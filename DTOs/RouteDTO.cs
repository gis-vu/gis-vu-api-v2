
using Helpers;

namespace DTOs
{
    public class RouteDTO
    {
        public RouteInfoDTO Info{ get; set; }
        public RouteDataDTO Data { get; set; }

        public string ToGeoJson()
        {
            return FeaturesToGeojsonHelper.ToGeojson(new []{Data.Coordinates});
        }
    }
}
