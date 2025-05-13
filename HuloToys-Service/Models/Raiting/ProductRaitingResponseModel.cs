namespace HuloToys_Service.Models.Raiting
{
    public class ProductRaitingRequestModel
    {
        public string id { get;set; }
        public int stars { get;set; }
        public bool has_comment { get;set; }
        public bool has_media { get;set; }
        public int page_index { get; set; }
        public int page_size { get; set; }
    }
    public class ProductRaitingResponseModel
    {
        public List<RatingESResponseModel> comments { get; set; }
        public Dictionary<int, long> comment_count_by_star { get; set; }
        public long has_comment_count { get; set; }
        public long has_media_count { get; set; }
        public long total_count { get; set; }
        public long total_sold { get; set; }
        public int page_index { get; set; }
        public int page_size { get; set; }
        public int max_page { get; set; }
    }
}
