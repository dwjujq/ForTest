namespace SilverSoft.Logging
{
    public interface IAppLogger<T>
    {
        public void InfoLog(string? message, params object?[] args);

        public void WarnLog(string? message, params object?[] args);

        public void DebugLog(string? message, params object?[] args);

        public void ErrorLog(Exception ex);

        public void ErrorLog(string? message, params object?[] args);

    }
}
