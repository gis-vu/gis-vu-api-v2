using DTOs;
using GIS.VU.API;
using LoadGIS;

namespace SearchGIS
{
    public class RouteRequest : RouteSearchRequestDTO
    {
        public RouteSearchRequestDTO Request { get; set; }
        public Graph Graph { get; set; }
        public LoadedData Data { get; set; }
    }
}