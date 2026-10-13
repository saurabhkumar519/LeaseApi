using LeaseBackend.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace LeaseBackend.DAL.Extension
{
    public static class ServiceExtention
    {
        public static void AddDataAccessLayerServices(this IServiceCollection services, IConfiguration configuration)
        {
            var Environment = configuration["DeploymentModes"]?.ToString().Trim();
            var connectstring = "";
            if (Environment == "DFCCIL")
            {
                connectstring = configuration.GetConnectionString("BuildingManagementProd");
            }
            else if (Environment == "DFCCIL_UAT")
            {
                connectstring = configuration.GetConnectionString("BuildingManagementDfcuat");
            }
            else
            {
                connectstring = configuration.GetConnectionString("BuildingManagementCetpa");
            }
            services.AddDbContext<TICKETTOKENContext>(options =>
            {

                options.UseSqlServer(connectstring);
            });
        }
    }
}
