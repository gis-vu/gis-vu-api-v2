using System.Collections.Generic;
using System.Linq;

namespace DTOs
{
    public class RouteSearchResponse
    {
        public Route[] Routes { get; set; }

        public RouteSearchResponse(IEnumerable<Route> routes)
        {
            Routes = routes.ToArray();
        }
    }
}
