namespace SilverSoft.Utils
{
    public static class FormFileExtensions
    {
        public static void SaveAs(this IFormFile file, string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) 
            { 
               Directory.CreateDirectory(dir);
            }

            var stream = file.OpenReadStream();
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }
    }
}
