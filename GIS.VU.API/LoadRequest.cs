using Models;

namespace GIS.VU.API
{
    public class LoadRequest
    {
        public PointPosition Start { get; set; }
        public PointPosition End { get; set; }
        public PointPosition[] Intermediates { get; set; }
    }
}