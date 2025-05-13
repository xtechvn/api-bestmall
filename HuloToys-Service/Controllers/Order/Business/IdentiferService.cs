using Caching.Elasticsearch;
using Entities.ConfigModels;
using HuloToys_Service.Utilities.constants.ClientType;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace HuloToys_Service.Controllers.Order.Business
{
    public class IdentiferService
    {
        private readonly IConfiguration _configuration;
        private readonly ClientESService clientESService;
        public IdentiferService( IConfiguration configuration)
        {
            _configuration = configuration;
            clientESService = new ClientESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
        }
        /// <summary>
        ///  Cấu trúc mã đơn hàng: năm + tháng + ngày + 5 số tăng dần (24082300001). Cứ hết năm sẽ reset 5 số tăng dần.
        /// </summary>
        /// <returns></returns>
        public async Task<string> buildOrderNo(long count_exists_order = 0)
        {
            try
            {
                if (count_exists_order > -1)
                {
                    count_exists_order++;
                    string order_no = string.Empty;
                    order_no += DateTime.Now.ToString("yyMMdd");
                    order_no += count_exists_order.ToString("D5");
                    return order_no;
                }

            }
            catch (Exception ex)
            {

            }
            string date = DateTime.Now.ToString("yyMMdd");
            var order_default = new Random().Next(1, 99999);
            return "HO" + date + order_default.ToString("D5");
        }
        public async Task<string> buildClientNo(int client_type)
        {
            string code = ClientTypeName.service[Convert.ToInt16(client_type)];

            try
            {
                var current_date = DateTime.Now;
                long count = clientESService.GetCountClientTypeUse(client_type);

                //so tu tang
                string s_format = string.Format(String.Format("{0,5:00000}", count + 1));

                //1. 2 số cuối của năm
                string two_year_last = current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);

                code = code + two_year_last + s_format;

                return code;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["telegram:log_try_catch:bot_token"], _configuration["telegram:log_try_catch:group_id"], error_msg);
                //Trả mã random
                var rd = new Random();
                var num_default = rd.Next(DateTime.Now.Day, DateTime.Now.Year) + rd.Next(1, 999);
                code = code + num_default;
                return code;
            }
        }
    }
}
