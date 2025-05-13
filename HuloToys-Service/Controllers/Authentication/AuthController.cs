// Create by: cuonglv
// Create date: 24-07-2024-
using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Models;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Utilities;
using Utilities.Contants;
namespace HuloToys_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly AccountApiESService accountApiESService;

        public AuthController(IConfiguration _configuration)
        {
            configuration = _configuration;
            accountApiESService = new AccountApiESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginModel user)
        {
            try
            {
                // Kiểm tra thông tin đăng nhập
                // Check trong ES xem user này có tồn tại hay không ?
                // đẩy data từ CMS lên Elasticsearch node accountAccessApi. Sau đó truy vấn lấy thông tin từ đó xuống
                if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Thông tin đăng nhập không đúng. Xin vui lòng thử lại"
                    });
                }

                var Password = CommonHelper.MD5Hash(user.Password);
                var accountClient = accountApiESService.GetByUsername(user.Username);

                if (accountClient == null) {
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Tài khoản " + user.Username + " không tồn tại" }); }
                if (accountClient.Status != (int)AccountClientStatusType.BINH_THUONG) { return Ok(new { status = (int)ResponseType.ERROR, msg = "Tài khoản đã khóa" }); }
                if (user.Username == accountClient.UserName && Password == accountClient.Password)
                {
                    var token = GenerateJwtToken(user.Username);
                    return Ok(new { status = (int)ResponseType.SUCCESS, token });
                }
                else
                {
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "Thông tin đăng nhập không hợp lệ" });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "Get account api with usr=" + user.Username + ". Error" + ex.ToString());

                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegram(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new { status = (int)ResponseType.ERROR, msg = "Thông tin đăng nhập không hợp lệ. Vui lòng liên hệ với Admin" });
            }
        }

        private string GenerateJwtToken(string username)
        {
            try
            {

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is my custom Secret key for authentication"));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: null,
                    audience: null,
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials);
                return new JwtSecurityTokenHandler().WriteToken(token);


            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return "";
            }
        }
    }
}
