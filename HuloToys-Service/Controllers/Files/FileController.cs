using HuloToys_Service.RedisWorker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HuloToys_Service.Controllers.Files
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class FileController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly RedisConn _redisService;

        public FileController(IConfiguration configuration, RedisConn redisService)
        {

            _configuration = configuration;
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [Route("upload")]
        public async Task<ActionResult> UploadFileAsync(IFormFile file)
        {
            if (file == null)
                return Ok(new { success = false, message = "You have to attach a file" });

            var fileName = file.FileName;
            // var extension = Path.GetExtension(fileName);

            // Add validations here...

            var localPath = $"{Path.Combine(System.AppContext.BaseDirectory, "myCustomDir")}\\{fileName}";

            // Create dir if not exists
            Directory.CreateDirectory(Path.Combine(System.AppContext.BaseDirectory, "myCustomDir"));

            using (var stream = new FileStream(localPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // db.SomeContext.Add(someData);
            // await db.SaveChangesAsync();

            return Ok(new { success = true, message = "All set", fileName });
        }
    }
}
