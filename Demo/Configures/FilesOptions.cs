namespace SilverSoft.Configures
{
    public class FilesOptions
    {
        public const string Files = "Files";

        public DemoOptions Demo { get; set; }
    }

    public class DemoOptions
    {
        public string UploadPath { get; set; }

        public string LogPath { get; set; }

        public string CSPReport { get; set; }
    }
}
