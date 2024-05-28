
using MUSbooking.Common.Caching;
using MUSbooking.Common.ExceptionHandler;
using MUSbooking.Common.Extentions;
using MUSbooking.Core;

namespace MUSbooking
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigurationBuilder config = new ConfigurationBuilder();
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("appsettings.json");
            IConfigurationRoot cfg = config.Build();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.ConfigureAutoMapper();

            builder.Services.ConfigureMediatR();

            builder.Services.ConfigureDbContext(cfg);

            builder.Services.AddScoped<CreateOrderService>();
            builder.Services.AddScoped<UpdateOrderService>();

            builder.Services.AddSingleton<Redis>();
                       
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "redishost";
                //"localhost:6379";
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCustomExceptionHandler();

            app.UseHttpsRedirection();
            
            app.UseRouting();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
