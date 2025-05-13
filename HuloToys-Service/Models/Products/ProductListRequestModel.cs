namespace HuloToys_Front_End.Models.Products
{
    public class ProductListRequestModel
    {
        public string keyword { get; set; }
        public int group_id { get; set; }
        public int page_index { get; set; }
        public int page_size { get; set; }
    }
}
