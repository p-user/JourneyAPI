using BusinessLayer.IServices;
using BusinessLayer.Services;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public static class DependencyInjection
    {
        public static void AddServicesBusiness(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IJourneyService, JourneyService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddAutoMapper(Assembly.GetAssembly(typeof(AutoMapperProfile)));
            services.AddTransient<IUrlHelperFactory, UrlHelperFactory>();
            services.AddScoped<INotificationService, NotificationService>();



        }
    }
}
