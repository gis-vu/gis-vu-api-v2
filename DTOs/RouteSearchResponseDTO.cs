using System;
using System.Collections.Generic;
using System.Linq;

namespace DTOs
{
    public class RouteSearchResponseDTO
    {
        public RouteDTO[] Routes { get; set; }
        public int StatusCode { get; set; } = 200;
        public string Message { get; set; } = "";

        public RouteSearchResponseDTO(IEnumerable<RouteDTO> routes)
        {
            Routes = routes.ToArray();
        }

        public RouteSearchResponseDTO(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
