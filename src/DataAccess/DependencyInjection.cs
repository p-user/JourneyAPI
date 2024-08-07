using DataAccessLayer.Config;
using DataAccessLayer.IRepositories;
using DataAccessLayer.Repositories;
using Microsoft.Extensions.DependencyInjection;
using SharedLayer.Nlog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public static class DependencyInjection
    {

        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerService, LoggerService>();
            services.AddSingleton<JwtConfig>();
            services.AddSingleton<UpdateAuditableInterceptor>();
            services.AddSingleton<SoftDeleteInterceptor>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IJourneyRepository, JourneyRepository>();
            services.AddScoped<IJourneyShareRepository, JourneyShareRepository>();

        }
    }
}
