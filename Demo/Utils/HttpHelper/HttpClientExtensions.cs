using Polly;

namespace SilverSoft.Utils.HttpHelper
{
    public static class HttpClientExtensions
    {
        public static void ConfigureHttpClient(this IServiceCollection services,IConfiguration configuration)
        {
            var httpConfigs = configuration.GetSection("HttpClientConfig").Get<List<HttpClientConfigOptions>>();
            HttpClientCommon.HttpClientConfigOptionsList = httpConfigs;

            httpConfigs.ForEach(cfg =>
            {
                services.AddHttpClient(cfg.Name)
                .AddPolicyHandler(request => Policy.TimeoutAsync<HttpResponseMessage>((cfg.Timeout==0||cfg.Timeout==-1)?System.Threading.Timeout.InfiniteTimeSpan:TimeSpan.FromSeconds(cfg.Timeout)))
                .AddTransientHttpErrorPolicy(p => p.RetryAsync(cfg.Retry));
            });
        }
    }
}
