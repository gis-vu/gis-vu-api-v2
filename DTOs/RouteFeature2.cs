using System;
using System.Collections.Generic;
using BAMCIS.GeoJSON;

namespace DTOs
{
    [Serializable]
    public class RouteFeature2
    {
        //public double Length { get; set; }
        //public Feature Feature { get; set; }
        public RoadFeature Feature { get; set; }
        public List<RouteFeature2> Neighbours { get; set; } = new List<RouteFeature2>();
    }
}
