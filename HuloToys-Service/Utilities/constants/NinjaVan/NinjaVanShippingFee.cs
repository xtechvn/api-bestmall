using static HuloToys_Service.Utilities.constants.NinjaVan.ShippingFeeModel;

namespace HuloToys_Service.Utilities.constants.NinjaVan
{
    public static class NinjaVanShippingFee
    {
        public enum ShippingAreaType
        {
            IN_TOWN = 1,
            IN_AREA = 2,
            BETWEEN_CITY = 3,
            BETWEEN_AREA = 4,
            BETWEEN_NEARBY_AREA = 5
        }

        public static List<int> WareHouse_Province_id = new List<int>() { 1 };
        public static List<int> AREA_NORTH_ProvinceId = new List<int>() { 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
        public static List<int> AREA_CENTER_ProvinceId = new List<int>() { 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44 };
        public static List<int> AREA_SOUTH_ProvinceId = new List<int>() { 45, 46, 47, 48, 49, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63 };
        public static int HCM_ProvinceId = 50;
        public static List<ShippingFeeModel> FEE = new List<ShippingFeeModel>
        {
            //--IN_TOWN
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_TOWN,min_weight=0,max_weight=500, amount=15 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_TOWN,min_weight=500,max_weight=1000, amount=16 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_TOWN,min_weight=1000,max_weight=2000, amount=16 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_TOWN,min_weight=2000,max_weight=4000, amount=17 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_TOWN,min_weight=4000,max_weight=5000, amount=24 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_TOWN,min_weight=5000,max_weight=7000, amount=33 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_TOWN,min_weight=7000,max_weight=10000, amount=43 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_TOWN,min_weight=10000,max_weight=20000, amount=77 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_TOWN,min_weight=20000,max_weight=-1, amount=77 },

           //--IN_AREA
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_AREA,min_weight=0,max_weight=500, amount=15 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_AREA,min_weight=500,max_weight=1000, amount=16 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_AREA,min_weight=1000,max_weight=2000, amount=16 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_AREA,min_weight=2000,max_weight=4000, amount=17 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_AREA,min_weight=4000,max_weight=5000, amount=24 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_AREA,min_weight=5000,max_weight=7000, amount=34 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_AREA,min_weight=7000,max_weight=10000, amount=46 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_AREA,min_weight=10000,max_weight=20000, amount=97 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.IN_AREA,min_weight=20000,max_weight=-1, amount=97 },

             //--BETWEEN_CITY (HN <-> HCM)
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_CITY,min_weight=0,max_weight=500, amount=16 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_CITY,min_weight=500,max_weight=1000, amount=16 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_CITY,min_weight=1000,max_weight=2000, amount=18 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_CITY,min_weight=2000,max_weight=4000, amount=22 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_CITY,min_weight=4000,max_weight=5000, amount=40 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_CITY,min_weight=5000,max_weight=7000, amount=47 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_CITY,min_weight=7000,max_weight=10000, amount=67 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_CITY,min_weight=10000,max_weight=20000, amount=177 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_CITY,min_weight=20000,max_weight=-1, amount=177 },


              //--BETWEEN_AREA
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_AREA,min_weight=0,max_weight=500, amount=17 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_AREA,min_weight=500,max_weight=1000, amount=17 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_AREA,min_weight=1000,max_weight=2000, amount=21 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_AREA,min_weight=2000,max_weight=4000, amount=24 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_AREA,min_weight=4000,max_weight=5000, amount=40 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_AREA,min_weight=5000,max_weight=7000, amount=57 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_AREA,min_weight=7000,max_weight=10000, amount=70 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_AREA,min_weight=10000,max_weight=20000, amount=227 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_AREA,min_weight = 20000, max_weight = -1, amount=227 },

              //--BETWEEN_NEARBY_AREA
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_NEARBY_AREA,min_weight=0,max_weight=500, amount=15 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_NEARBY_AREA,min_weight=500,max_weight=1000, amount=16 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_NEARBY_AREA,min_weight=1000,max_weight=2000, amount=17 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_NEARBY_AREA,min_weight=2000,max_weight=4000, amount=20 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_NEARBY_AREA,min_weight=4000,max_weight=5000, amount=40 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_NEARBY_AREA,min_weight=5000,max_weight=7000, amount=57 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_NEARBY_AREA,min_weight=7000,max_weight=10000, amount=70 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_NEARBY_AREA,min_weight=10000,max_weight=20000, amount=147 },
            new ShippingFeeModel(){ area=(int)ShippingAreaType.BETWEEN_NEARBY_AREA,min_weight = 20000, max_weight = 10 * 1000 * 1000, amount=147 },

        };
        public static List<ShippingAdditionalFeeModel> ADDITIONAL_FEE = new List<ShippingAdditionalFeeModel>()
        {
           new ShippingAdditionalFeeModel(){ area=(int)ShippingAreaType.IN_TOWN,unit=1000, amount=3 },
           new ShippingAdditionalFeeModel(){ area=(int)ShippingAreaType.IN_AREA,unit=1000, amount=4 },
           new ShippingAdditionalFeeModel(){ area=(int)ShippingAreaType.BETWEEN_CITY,unit=1000, amount=7 },
           new ShippingAdditionalFeeModel(){ area=(int)ShippingAreaType.BETWEEN_AREA,unit=1000, amount=8 },
           new ShippingAdditionalFeeModel(){ area=(int)ShippingAreaType.BETWEEN_NEARBY_AREA,unit=1000, amount=6 },

        };
        public static int MAX_STANDARD_WEIGHT = 20000;
        public static int RATE = 1000;
    }
    public class ShippingFeeModel
    {
        public int area { get; set; }
        public int amount { get; set; }
        public int min_weight { get; set; }
        public int max_weight { get; set; }
    }
    public class ShippingAdditionalFeeModel
    {
        public int area { get; set; }
        public int amount { get; set; }
        public int unit { get; set; }
    }
   
}
