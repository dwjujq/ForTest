namespace SilverSoft.Utils.HttpHelper
{
    public class HttpFileUploadInfo
    {
        public string FileName { get; set; }

        public Stream Stream { get; set; }

        public string Extension { get; set; }

        private static readonly Dictionary<string, string> fileExtensionDic = new()
        {
            { "JPG", "255216" },
            { "GIF", "7173" },
            { "BMP", "6677" },
            { "PNG", "13780" },
            { "COM", "7790" },
            { "EXE", "7790" },
            { "DLL", "7790" },
            { "RAR", "8297" },
            { "ZIP", "8075" },
            { "XML", "6063" },
            { "HTML", "6033" },
            { "ASPX", "239187" },
            { "CS", "117115" },
            { "JS", "119105" },
            { "TXT", "210187" },
            { "SQL", "255254" },
            { "BAT", "64101" },
            { "BTSEED", "10056" },
            { "RDP", "255254" },
            { "PSD", "5666" },
            { "PDF", "3780" },
            { "CHM", "7384" },
            { "LOG", "70105" },
            { "REG", "8269" },
            { "HLP", "6395" },
            { "DOC", "208207" },
            { "XLS", "208207" },
            { "DOCX", "208207" },
            { "XLSX", "208207" },
            { "CSV", "4949" }
        };

        public static HttpFileUploadInfo Parse(IFormFile file, List<string> allowedExtensions)
        {
            if (file == null)
            {
                return null;
            }

            var fileUploadInfo = new HttpFileUploadInfo();
            fileUploadInfo.FileName = file.FileName;
            fileUploadInfo.Extension = Path.GetExtension(file.FileName);
            fileUploadInfo.Stream = file.OpenReadStream();

            #region 验证文件类型
            System.IO.BinaryReader r = new System.IO.BinaryReader(fileUploadInfo.Stream);

            string bx = "";
            byte buffer;
            try
            {
                buffer = r.ReadByte();
                bx = buffer.ToString();
                buffer = r.ReadByte();
                bx += buffer.ToString();
                fileUploadInfo.Stream.Seek(0, SeekOrigin.Begin);
                if (allowedExtensions?.Count > 0)
                {
                    if (allowedExtensions.Any(e => fileExtensionDic[e] == bx))
                    {
                        return fileUploadInfo;
                    }
                    return null;
                }
                return fileUploadInfo;
            }
            catch (Exception ex)
            {
                return null;
            }
            #endregion
        }

        public static bool TryParse(IFormFile file, List<string> allowedExtensions, out HttpFileUploadInfo fileUploadInfo)
        {
            try
            {
                fileUploadInfo = Parse(file, allowedExtensions);
                if (fileUploadInfo == null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                fileUploadInfo = null;
                return false;
            }
        }

        public static HttpFileUploadInfo ParseSingle(HttpRequest request,string name=null, List<string> allowedExtensions=null)
        {
            var file =string.IsNullOrWhiteSpace(name)? request.Form.Files.FirstOrDefault(): request.Form.Files[name];

            return Parse(file, allowedExtensions);
        }

        public static bool TryParseSingle(HttpRequest request,string name, List<string> allowedExtensions, out HttpFileUploadInfo fileUploadInfo)
        {
            try
            {
                fileUploadInfo = ParseSingle(request,name, allowedExtensions);
                if (fileUploadInfo == null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                fileUploadInfo = null;
                return false;
            }
        }

        public static bool IsAllowedExtension(Stream fileStream, List<string> allowedExtensions=null)
        {
            #region 验证文件类型
            System.IO.BinaryReader r = new System.IO.BinaryReader(fileStream);

            string bx = "";
            byte buffer;
            try
            {
                buffer = r.ReadByte();
                bx = buffer.ToString();
                buffer = r.ReadByte();
                bx += buffer.ToString();
                fileStream.Seek(0, SeekOrigin.Begin);
                if (allowedExtensions?.Count > 0)
                {
                    if (allowedExtensions.Any(e => fileExtensionDic[e] == bx))
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            #endregion
        }

        public static bool IsAllowedExtension(IFormFile file, List<string> allowedExtensions = null)
        {
            if(file==null)
            {
                return false;
            }
            return IsAllowedExtension(file.OpenReadStream(), allowedExtensions);
        }
    }
}
