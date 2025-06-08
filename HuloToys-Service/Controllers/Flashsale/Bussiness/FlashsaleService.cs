using Entities.ViewModels.Products;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.Models.Flashsale;
using Utilities.Contants;

namespace HuloToys_Service.Controllers.Flashsale.Bussiness
{
    public class FlashsaleService
    {
        private readonly IConfiguration _configuration;

        public FlashsaleService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
       
    }
}
