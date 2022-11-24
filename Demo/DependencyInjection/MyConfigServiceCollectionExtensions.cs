using SilverSoft.Configures;
using SilverSoft.Utils;

namespace Microsoft.Extensions.DependencyInjection;

public static class MyConfigServiceCollectionExtensions
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<FilesOptions>(config.GetSection(FilesOptions.Files));

        GlobalVariables.FilesOptions = config.GetSection(FilesOptions.Files).Get<FilesOptions>();

        GlobalVariables.Configuration = config;

        return services;
    }
}
