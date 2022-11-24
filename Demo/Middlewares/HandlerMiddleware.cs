using System.Reflection;
using Microsoft.Net.Http.Headers;
using SilverSoft.Utils;

namespace SilverSoft.Middlewares
{
    public static class HandlerMiddleware
    {
        private static Dictionary<string, Type> routeHandlerTypeDic = new Dictionary<string, Type>();

        public static void UseHandlerMiddleware(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            try
            {
                app.MapWhen(context => context.Request.Path.Value.EndsWith(".ashx") 
                && !context.Request.Path.Value.EndsWith("SilverSoft.Robot.ashx")
                && !context.Request.Path.Value.EndsWith("MicroServer.ashx"), GetHandler);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private static void GetHandler(IApplicationBuilder app)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var handlerTypes = assembly.GetTypes().Where(t => t.GetInterfaces().Any(i => typeof(IHttpHandler).IsAssignableFrom(i)));
            foreach (var handlerType in handlerTypes)
            {
                var handlerRouteAtrribute = (HandlerRouteAttribute)handlerType.GetCustomAttribute(typeof(HandlerRouteAttribute));
                if (handlerRouteAtrribute != null)
                {
                    routeHandlerTypeDic.TryAdd(handlerRouteAtrribute.Route.ToLower(), handlerType);
                }
                else
                {
                    routeHandlerTypeDic.TryAdd(handlerType.FullName.ToLower(), handlerType);
                }
            }
            app.Run(async (context) =>
            {
                context.Response.OnStarting((state) =>
                {
                    if (!context.Response.Headers.ContainsKey(HeaderNames.ContentType))
                    {
                        context.Response.Headers.Append(HeaderNames.ContentType, "text/html;charset=utf-8");
                    }
                    else
                    {
                        var contentType = context.Response.Headers[HeaderNames.ContentType].ToString();
                        if ((contentType.StartsWith("application/json",StringComparison.OrdinalIgnoreCase)|| contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)) 
                        && !contentType.Contains("charset",StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.Headers.Remove(HeaderNames.ContentType);
                            context.Response.Headers.Append(HeaderNames.ContentType, $"{contentType};charset=utf-8");
                        }
                    }

                    return Task.FromResult(0);
                }, null);

                if (routeHandlerTypeDic.TryGetValue(context.Request.Path.Value.ToLower(), out var handlerType))
                {
                    var handler = (IHttpHandler)Activator.CreateInstance(handlerType);

                    handler.ProcessRequest(context);
                }
                await Task.CompletedTask;
            });

        }
    }
}
