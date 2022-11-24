using SilverSoft.Configures;

namespace SilverSoft.Utils
{
    public class GlobalVariables
    {   
        public static ILoggerFactory LoggerFactory { get; set; }

        public static FilesOptions FilesOptions { get; set; }

        public static IConfiguration Configuration { get; set; }
    }
}
