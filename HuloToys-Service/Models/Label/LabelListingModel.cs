namespace API_CORE.Controllers.Models.Label
{
    public class LabelListingModel : API_CORE.Controllers.Models.Models.Label
    {
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public int TotalRow { get; set; }
    }
}
