using ChattingApplication.Data;
using ChattingApplication.Helpers;
using ChattingApplication.Interfaces;
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

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.Configure<CloudSetting>(config.GetSection("CloudinarySetting"));
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<ILikeRepository, LikeRepository>();
            services.AddScoped<LogUserActivity>();
            return services;
        }
    }
}
