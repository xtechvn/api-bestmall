namespace HuloToys_Service.Models.Label
{
    public class LabelListingModel : HuloToys_Service.Models.Models.Label
    {
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public int TotalRow { get; set; }
    }
}
