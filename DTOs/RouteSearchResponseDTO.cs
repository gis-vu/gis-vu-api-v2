using System.Collections.Generic;
using System.Linq;

namespace DTOs
{
    public class RouteSearchResponseDTO
    {
        public RouteDTO[] Routes { get; set; }

        public RouteSearchResponseDTO(IEnumerable<RouteDTO> routes)
        {
            Routes = routes.ToArray();
        }
    }
}
