using System;
using System.Collections.Generic;
using BAMCIS.GeoJSON;

namespace DTOs
{
    [Serializable]
    public class RoadFeature
    {
        public CustomPosition[] Coordinates { get; set; }
        public IDictionary<string, dynamic> Properties { get; set; }
    }
}