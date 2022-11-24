using log4net;
using log4net.Appender;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.StaticFiles;
using SilverSoft.Logging;
using SilverSoft.Middlewares;
using SilverSoft.Utils;
using SilverSoft.Utils.HttpHelper;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using WebMarkupMin.AspNet.Brotli;
using WebMarkupMin.AspNet.Common.Compressors;
using WebMarkupMin.AspNetCore6;
using WebMarkupMin.Core;
using WebMarkupMin.NUglify;
using static System.Net.Mime.MediaTypeNames;
using IWmmLogger = WebMarkupMin.Core.Loggers.ILogger;
using WmmThrowExceptionLogger = WebMarkupMin.Core.Loggers.ThrowExceptionLogger;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                       optional: true,
                       reloadOnChange: true);
});
builder.Services.AddConfig(builder.Configuration);

#region 日志配置
builder.Services.AddLogging(cfg =>
{
    cfg.AddLog4Net();
});

var repository = LogManager.GetRepository();
var appenders = repository.GetAppenders();
foreach (RollingFileAppender appender in appenders)
{
    appender.File = GlobalVariables.FilesOptions.Demo.LogPath;
    appender.ActivateOptions();
}
#endregion


var useMarkup = builder.Configuration.GetValue<bool>("UseMarkup", false);

// 配置跨域处理
builder.Services.AddCors(options =>
    options.AddPolicy("cors", p => {
        if (builder.Environment.IsDevelopment())
        {
            p.AllowAnyOrigin();
        }
        else
        {
            p.WithOrigins("https://www.test.com");
        }
        p.WithHeaders("Content-Type").WithMethods("GET", "POST");
    })
);

builder.Services.AddRazorPages();
builder.Services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));
builder.Services.AddDetection();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

#region 配置WebMarkupMin
if (useMarkup)
{
    builder.Services.AddWebMarkupMin(options =>
    {
        options.AllowMinificationInDevelopmentEnvironment = true;//本地html调试必须设置为true，否则不生效
        options.AllowCompressionInDevelopmentEnvironment = true;//本地API调试必须设置为true，否则不生效
    })
    .AddHtmlMinification(options =>
    { //Html压缩
        HtmlMinificationSettings settings = options.MinificationSettings;
        settings.RemoveHtmlComments = false;
        settings.RemoveHtmlComments­FromScriptsAndStyles = false;
        settings.MinifyInlineCssCode = false;//页面内css压缩
        settings.MinifyInlineJsCode = false;//页面内js压缩
        options.CssMinifierFactory = new NUglifyCssMinifierFactory();
        options.JsMinifierFactory = new NUglifyJsMinifierFactory();
    })
    .AddHttpCompression(options =>
    {
        options.CompressorFactories = new List<ICompressorFactory>
        {
            new BrotliCompressorFactory(new BrotliCompressionSettings
            {
                 Level = (int)CompressionLevel.Fastest
            }),
            new GZipCompressorFactory(new GZipCompressionSettings
            {
                Level = CompressionLevel.Fastest
            }),
            new DeflateCompressorFactory(new DeflateCompressionSettings
            {
                Level = CompressionLevel.Fastest
            })
        };
    });

    // Override the default logger for WebMarkupMin.
    builder.Services.AddSingleton<IWmmLogger, WmmThrowExceptionLogger>();
}
#endregion

// Add services to the container.
builder.Services.AddControllersWithViews().AddMvcOptions(options =>
{
    options.InputFormatters.OfType<SystemTextJsonInputFormatter>().First()
    .SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/csp-report"));
});

//Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.AccessDeniedPath = "/Home/Index";
    options.LoginPath = "/Home/Index";
});
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

builder.Services.ConfigureHttpClient(builder.Configuration);

#region 设置可以用同步的方法写入响应流，设置上传文件大小限制
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
    options.Limits.MaxRequestBodySize = int.MaxValue;
    options.AddServerHeader = false;
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
    options.MaxRequestBodySize = int.MaxValue;
});

builder.Services.Configure<FormOptions>(x =>
{
    x.MultipartBodyLengthLimit = int.MaxValue;
});
#endregion

var app = builder.Build();

var logger = app.Logger;
logger.LogInformation(useMarkup ? "Use markup" : "Not use markup");

// 允许跨域，cors是跨域策略名称
app.UseCors("cors");

app.UseSession();

#region 处理异常

if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/ApplicationError");
    app.UseStatusCodePagesWithReExecute("/Error{0}");
}
#endregion

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//var policyCollection = new HeaderPolicyCollection()
//        .AddFrameOptionsSameOrigin()
//        .AddXssProtectionBlock()
//        .AddContentTypeOptionsNoSniff()
//        .AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 60 * 60 * 24 * 365) // maxage = one year in seconds
//        .AddReferrerPolicyStrictOriginWhenCrossOrigin()
//        .RemoveServerHeader()
//        .AddContentSecurityPolicyReportOnly(builder =>
//        {
//            builder.AddReportUri().To("/cspreport");
//            builder.AddDefaultSrc().Self();
//            //builder.AddObjectSrc().Self();
//            //builder.AddFormAction().Self();
//            //builder.AddFrameAncestors().Self();
//        });
////.AddContentSecurityPolicy(builder =>
////{
////    builder.AddReportUri().To("/cspreport");
////    builder.AddObjectSrc().Self();
////    builder.AddFormAction().Self();
////    builder.AddFrameAncestors().Self();
////});

//app.UseSecurityHeaders(policyCollection);

app.UseSecurityHeadersMiddleware(new SecurityHeadersBuilder().AddDefaultSecurePolicy());

app.UseHandlerMiddleware();
app.MapRazorPages();

var provider = new FileExtensionContentTypeProvider();
// Add new mappings
provider.Mappings[".properties"] = "text/plain";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

if (useMarkup)
{
    app.UseWebMarkupMin();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var httpClientFactory = app.Services.GetRequiredService<IHttpClientFactory>();
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US", true) { DateTimeFormat = { ShortDatePattern = "yyyy/MM/dd", FullDateTimePattern = "yyyy/MM/dd HH:mm:ss", LongTimePattern = "HH:mm:ss" } };

HttpClientCommon.HttpClientFactory = httpClientFactory;
GlobalVariables.LoggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
LogHelper.Init(GlobalVariables.LoggerFactory);

app.Run();
