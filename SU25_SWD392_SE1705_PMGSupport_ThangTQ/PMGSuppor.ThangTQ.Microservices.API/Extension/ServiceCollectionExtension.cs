using PMGSupport.ThangTQ.Services;

namespace PMGSuppor.ThangTQ.Microservices.API.Extension;

public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IGradeService, GradeService>();
        
        return services;
    }
}