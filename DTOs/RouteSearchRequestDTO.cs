using System.Linq;

namespace DTOs
{
    public class RouteSearchRequestDTO
    {
        public CoordinateDTO Start { get; set; }
        public CoordinateDTO End { get; set; }
        public CoordinateDTO[] Points { get; set; }
        public CoordinateDTO[] PolygonPoints { get; set; }
        public SearchOptionsDTO SearchOptions { get; set; }

        public LoadRequest ToLoadRequest()
        {
            return new LoadRequest()
            {
               Start =   new[] { Start.Lng, Start.Lat},
               End =   new[] { End.Lng, End.Lat},
               Intermediates =   Points.Select(x=> new[]{x.Lng,x.Lat}).ToArray()
            };
        }
    }
}
