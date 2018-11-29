namespace DTOs
{
    public class SearchOptionsDTO
    {
        public PropertyImportanceDTO[] PropertyImportance { get; set; }
        public PropertyValueImportanceDTO[] PropertyValueImportance { get; set; }
        public double TrackOverlapImportance { get; set; }
    }
}