using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using Entities.ConfigModels;
using HuloToys_Service.Controllers.Flashsale.Bussiness;
using HuloToys_Service.Controllers.IRepositories;
using HuloToys_Service.Controllers.Payment.Bussiness;
using HuloToys_Service.Controllers.Product.Bussiness;
using HuloToys_Service.Controllers.Repositories;
using HuloToys_Service.Controllers.Shipping.Business;
using HuloToys_Service.IRepositories;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Models.Models;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Repositories;
using HuloToys_Service.Utilities.Middleware;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepositories;
using Repositories.Repositories;
using REPOSITORIES.IRepositories;
using REPOSITORIES.Repositories;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configure JWT Authentication
        //var key = "this is my custom Secret key for authentication";
        //builder.Services.AddAuthentication(options =>
        //{
        //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //}).AddJwtBearer(options =>
        //{
        //    options.TokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuer = false,
        //        ValidateAudience = false,
        //        ValidateLifetime = true,
        //        ValidateIssuerSigningKey = true,
        //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        //    };
        //});
        var Configuration = builder.Configuration;
        builder.Services.Configure<DataBaseConfig>(Configuration.GetSection("DataBaseConfig"));
        builder.Services.Configure<MailConfig>(Configuration.GetSection("MailConfig"));
        builder.Services.Configure<DomainConfig>(Configuration.GetSection("DomainConfig"));
        // ??ng k� ApplicationDbContext
        var connectionString = builder.Configuration.GetSection("DataBaseConfig:SqlServer:ConnectionString").Value;
        builder.Services.AddDbContext<DataMSContext>(options =>
                options.UseSqlServer(connectionString));

        // Register services
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddSingleton<IClientRepository, ClientRepository>();
        builder.Services.AddSingleton<IAllCodeRepository, AllCodeRepository>();
        builder.Services.AddSingleton<IAccountClientRepository, AccountClientRepository>();
        builder.Services.AddSingleton<IProvinceRepository, ProvinceRepository>();
        builder.Services.AddSingleton<IDistrictRepository, DistrictRepository>();
        builder.Services.AddSingleton<IWardRepository, WardRepository>();
        builder.Services.AddSingleton<ILabelRepository, LabelRepository>();
        builder.Services.AddSingleton<IAddressClientRepository, AddressClientRepository>();
        builder.Services.AddSingleton<IVoucherRepository, VoucherRepository>();
        builder.Services.AddSingleton<IAllCodeRepository, AllCodeRepository>();
        builder.Services.AddSingleton<IContractPayRepository, ContractPayRepository>();
        builder.Services.AddSingleton<IIdentifierServiceRepository, IdentifierServiceRepository>();
        builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
        builder.Services.AddSingleton<IVoucherRepository, VoucherRepository>();
        builder.Services.AddSingleton<IOrderMergeRepository, OrderMergeRepository>();
        builder.Services.AddSingleton<IBankingAccountRepository, BankingAccountRepository>();
        builder.Services.AddSingleton<IAllotmentFundRepository, AllotmentFundRepository>();
        builder.Services.AddSingleton<IAllotmentUseRepository, AllotmentUseRepository>();
        builder.Services.AddSingleton<ProductDetailService>();
        builder.Services.AddSingleton<ProductRaitingService>();
        builder.Services.AddSingleton<CartMongodbService>();
        builder.Services.AddSingleton<OrderMongodbService>();
        builder.Services.AddSingleton<ProductDetailMongoAccess>();
        builder.Services.AddSingleton<ProductFavouritesMongoAccess>();
        builder.Services.AddSingleton<ProductSpecificationMongoAccess>();
        builder.Services.AddSingleton<NewsMongoService>();
        builder.Services.AddSingleton<ClientContactMongodbService>();
        builder.Services.AddSingleton<SupplierESRepository>();
        builder.Services.AddSingleton<FlashsaleService>();


        builder.Services.AddSingleton<RedisConn>();
        builder.Services.AddSingleton<ViettelPostService>();
        builder.Services.AddSingleton<VNPayService>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins", policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost",
                        "https://localhost",
                        "http://best-mall.vn",
                        "https://best-mall.vn",
                        "http://bestmall.com.vn",
                        "https://bestmall.com.vn"
                    )
                    .SetIsOriginAllowed(origin =>
                        origin.StartsWith("http://localhost") ||
                        origin.StartsWith("https://localhost") ||
                        origin.EndsWith(".best-mall.vn") ||
                        origin.EndsWith(".bestmall.com.vn") ||
                        origin == "http://best-mall.vn" ||
                        origin == "https://best-mall.vn" ||
                        origin == "http://bestmall.com.vn" ||
                        origin == "https://bestmall.com.vn"
                    )
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST", "OPTIONS");
            });
        });
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseCors("AllowSpecificOrigins");

        app.UseHttpsRedirection();

        app.UseAuthorization();

       // app.UseMiddleware<GlobalErrorCaptureMiddleware>();

        app.MapControllers();

        app.Run();
    }
}