namespace SilverSoft.Logging
{
    public class AppLogger<T> : IAppLogger<T>
    {
        private readonly ILogger<T> _logger;
        public AppLogger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<T>();
        }

        public void InfoLog(string? message, params object?[] args) => _logger.LogInformation(message, args);

        public void WarnLog(string? message, params object?[] args) => _logger.LogWarning(message, args);

        public void DebugLog(string? message, params object?[] args) => _logger.LogDebug(message, args);

        public void ErrorLog(Exception ex)=> _logger.LogError(ex,string.Empty);

        public void ErrorLog(string? message, params object?[] args) => _logger.LogError(message, args);

    }
}
