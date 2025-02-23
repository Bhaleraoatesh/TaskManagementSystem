using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Domain.Repository;
using TaskManagement.Persistance.Repository;

namespace TaskManagement.Persistance.Extensions
{
    public static class ServiceRegistration
    {
        // Extension method for IServiceCollection to register services and bind configurations
        public static void AddServiceRegistration(this IServiceCollection services, IConfiguration configuration)
        {
            // Register repositories
            services.AddScoped<IRepository, TaskmanagementRepository>();
        }
    }
}
