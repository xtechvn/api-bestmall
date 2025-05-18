using HuloToys_Service.Models.NinjaVan;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.constants.NinjaVan;
using HuloToys_Service.Utilities.constants.Shipping;

namespace HuloToys_Service.Controllers.Shipping.Business
{
    public class ShippingBussinessSerice
    {
        private readonly IConfiguration configuration;
        private readonly NinjaVanService ninjaVanService;
        private readonly ProductDetailMongoAccess _productDetailMongoAccess;
        private readonly CartMongodbService _cartMongodbService;

        public ShippingBussinessSerice(IConfiguration _configuration)
        {
            configuration = _configuration;
            ninjaVanService = new NinjaVanService(configuration);
            _productDetailMongoAccess = new ProductDetailMongoAccess(configuration);
            _cartMongodbService = new CartMongodbService(configuration);

        }
        public async Task<ShippingFeeResponseModel> GetShippingFeeResponse(ShippingFeeRequestModel request)
        {
            try
            {
                var response = new ShippingFeeResponseModel()
                {
                    from_province_id = request.from_province_id<=0 ? NinjaVanShippingFee.WareHouse_Province_id.First(): request.from_province_id,
                    to_province_id = request.to_province_id,
                    total_shipping_fee = 0,
                    type = request.shipping_type,

                };
                //-- Delivery type
                switch (request.shipping_type)
                {
                    case (int)ShippingType.FAST_DELIVERY:
                        {
                            //-- switch delivery carrier
                            switch (request.carrier_id)
                            {
                                case (int)ShippingCarrier.NINJAVAN:
                                    {
                                        float total_weight = 0;
                                        foreach (var cart in request.carts)
                                        {
                                            var cart_detail = await _cartMongodbService.FindById(cart.id);
                                            var product = await _productDetailMongoAccess.GetByID(cart.product_id);
                                            if (product != null && product.weight != null && (float)product.weight >0)
                                            {
                                                total_weight += ((float)product.weight * cart.quanity);
                                            }

                                        }
                                        response.total_shipping_fee = ninjaVanService.CaclucateShippingFee(request.to_province_id, Convert.ToInt32(total_weight));
                                       
                                    }
                                    break;
                            }

                        }
                        break;
                    case (int)ShippingType.SLOW_DELIVERY:
                        {
                            //-- switch delivery carrier
                            switch (request.carrier_id)
                            {
                                case (int)ShippingCarrier.NINJAVAN:
                                    {
                                        float total_weight = 0;
                                        foreach (var cart in request.carts)
                                        {
                                            var cart_detail = await _cartMongodbService.FindById(cart.id);
                                            var product = await _productDetailMongoAccess.GetByID(cart.product_id);
                                            if (product != null && product.weight != null && (float)product.weight > 0)
                                            {
                                                total_weight += ((float)product.weight * cart.quanity);
                                            }

                                        }
                                        response.total_shipping_fee = ninjaVanService.CaclucateShippingFee(request.to_province_id, Convert.ToInt32(total_weight));

                                    }
                                    break;
                            }

                        }
                        break;
                    case (int)ShippingType.INSTANT_DELIVERY:
                        {


                        }
                        break;
                    case (int)ShippingType.RECEIVER_AT_WAREHOUSE:
                        {
                            
                        }
                        break;
                    case (int)ShippingType.COD:
                        {
                            //-- switch delivery carrier
                            switch (request.carrier_id)
                            {
                                case (int)ShippingCarrier.NINJAVAN:
                                    {
                                        float total_weight = 0;
                                        foreach (var cart in request.carts)
                                        {
                                            var cart_detail = await _cartMongodbService.FindById(cart.id);
                                            var product = await _productDetailMongoAccess.GetByID(cart.product_id);
                                            if (product != null && product.weight != null && (float)product.weight > 0)
                                            {
                                                total_weight += ((float)product.weight * cart.quanity);
                                            }

                                        }
                                        response.total_shipping_fee = ninjaVanService.CaclucateShippingFee(request.to_province_id, Convert.ToInt32(total_weight));

                                    }
                                    break;
                            }
                        }
                        break;

                }
                return response;
            }
            catch
            {

            }
            return null;
        }
    }
}
