using Microsoft.Extensions.DependencyInjection;
using podfy_media_counter_application.Context;

namespace podfy_media_counter_application.IoC;

internal static class ConfigureServices
{
    public static void AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IDynamoContext, DynamoContext>();
    }
}

