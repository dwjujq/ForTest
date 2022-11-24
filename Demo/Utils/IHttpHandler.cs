namespace SilverSoft.Utils
{
    public interface IHttpHandler
    {
        bool IsReusable
        {
            get;
        }

        void ProcessRequest(HttpContext context);
    }
}
