﻿using ChattingApplication.Data;
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
            return services;
        }
    }
}