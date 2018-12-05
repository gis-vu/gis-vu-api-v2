
namespace DTOs
{
    public class LoadRequest
    {
        public double[] Start { get; set; }
        public double[] End { get; set; }
        public double[][] Intermediates { get; set; }
    }
}