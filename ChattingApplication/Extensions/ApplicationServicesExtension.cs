using ChattingApplication.Data;
using ChattingApplication.Helpers;
using ChattingApplication.Interfaces;
using ChattingApplication.SignalR;
using ChattingApplication.Srvices;
using ChattingApplication.Srvices.TokenServics;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace ChattingApplication.Extensions
{
    public static class ApplicationServicesExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<DataContext>(opt => opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));
            services.AddCors();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.Configure<CloudSetting>(config.GetSection("CloudinarySetting"));
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<LogUserActivity>();

            services.AddSignalR();
            services.AddSingleton<PrecenseTracker>();
            return services;
        }
    }
}
