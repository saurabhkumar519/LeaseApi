using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LeaseBackend.BAL.IServices;
using LeaseBackend.BAL.Services;
using LeaseBackend.DAL.DapperServices;

namespace LeaseBackend.BAL.Extension
{
    public static class ServiceExtension
    {
        public static void AddBusinessLayerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IAccountService, AccountRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUploadService, UploadService>();
           
            services.AddScoped<IDapperService, DapperService>();
            

        }
    }
}
