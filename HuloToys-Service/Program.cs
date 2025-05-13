using Entities.ConfigModels;
using HuloToys_Service.IRepositories;
using HuloToys_Service.Models.Models;
using HuloToys_Service.RedisWorker;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepositories;
using Repositories.Repositories;

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
        // ??ng ký ApplicationDbContext
        var connectionString = builder.Configuration.GetSection("DataBaseConfig:SqlServer:ConnectionString").Value;
        builder.Services.AddDbContext<DataMSContext>(options =>
                options.UseSqlServer(connectionString));


        // Register services
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddSingleton<IClientRepository, ClientRepository>();
        builder.Services.AddSingleton<IAccountClientRepository, AccountClientRepository>();
        builder.Services.AddSingleton<IProvinceRepository, ProvinceRepository>();
        builder.Services.AddSingleton<IDistrictRepository, DistrictRepository>();
        builder.Services.AddSingleton<IWardRepository, WardRepository>();
        builder.Services.AddSingleton<ILabelRepository, LabelRepository>();


        builder.Services.AddSingleton<RedisConn>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}