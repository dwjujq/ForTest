using System.Diagnostics;
using System.Text;

namespace SilverSoft.Logging
{
    public class LogHelper
    {
        private static ILogger<LogHelper> _logger;

        public static void Init(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LogHelper>();
        }

        private static void WriteLog(LogLevel logLevel, Exception? exception, string? message, params object?[] args)
        {
            if(_logger==null)
            {
                throw new Exception("请先调用初始化方法");
            }

            var stackTrace = new StackTrace();
            var stackFrame = stackTrace.GetFrame(2);
            var callingMethod = stackFrame?.GetMethod();

            var logInfoBuilder = new StringBuilder();

            logInfoBuilder.AppendLine($"[{callingMethod?.DeclaringType?.FullName ?? "?"}][{callingMethod?.Name ?? "?"}]");
            logInfoBuilder.Append(message ?? string.Empty);

            _logger.Log(logLevel, exception, logInfoBuilder.ToString(), args);
        }

        public static void InfoLog(string? message, params object?[] args)=>WriteLog(LogLevel.Information,null,message,args);

        public static void WarnLog(string? message, params object?[] args) => WriteLog(LogLevel.Warning, null, message, args);

        public static void DebugLog(string? message, params object?[] args) => WriteLog(LogLevel.Debug, null, message, args);

        public static void ErrorLog(Exception ex) => WriteLog(LogLevel.Error,ex,string.Empty);

        public static void ErrorLog(string? message, params object?[] args) => WriteLog(LogLevel.Error, null, message, args);
    }
}
