using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;
using SilverSoft.Logging;
using System.Text;

namespace SilverSoft.Utils
{
    public static class HttpRequestExtensions
    {
        private static Encoding GetEncodingFromCharset(string charset)
        {
            if(string.IsNullOrWhiteSpace(charset)||charset.Equals("utf-8", StringComparison.OrdinalIgnoreCase))
            {
                return Encoding.UTF8;
            }

            try
            {
                return Encoding.GetEncoding(charset);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"charset '{charset}' is not a known encoding.", ex);
            }
        }

        public static string GetCharset(this HttpRequest request)
        {
            if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var mt))
            {
                return null;
            }
            return mt.Charset.Value;
        }

        public static string GetString(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            using(var ms=new MemoryStream())
            {
                try
                {
                    if (request.Body.CanSeek)
                    {
                        request.Body.Seek(0, SeekOrigin.Begin);
                    }
                    request.Body.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    var encoding = GetEncodingFromCharset(request.GetCharset());

                    return encoding.GetString(ms.ToArray());
                }
                finally
                {
                    if (request.Body.CanSeek)
                    {
                        request.Body.Seek(0, SeekOrigin.Begin);
                    }
                }
            }
        }


        public static string GetRequestInfo(this HttpRequest request,bool forceLogBody=false)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                var requestInfo = new StringBuilder();
                requestInfo.AppendLine($"Url:{request.GetDisplayUrl()}");

                requestInfo.AppendLine("Header:");
                if (request.Headers.Count > 0)
                {
                    foreach (var header in request.Headers)
                    {
                        requestInfo.AppendLine($"{header.Key}:{header.Value}");
                    }
                }

                requestInfo.AppendLine("Body:");
                if (request.ContentType != null &&
                    (request.ContentType.StartsWith("text/",StringComparison.OrdinalIgnoreCase)
                    || request.ContentType.Equals("application/xml",StringComparison.OrdinalIgnoreCase)
                    || request.ContentType.Equals("application/json", StringComparison.OrdinalIgnoreCase)
                    || request.ContentType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)) || forceLogBody)
                {
                    requestInfo.AppendLine(request.GetString());
                }

                return requestInfo.ToString();
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
                return "Error Occured";
            }
        }
    }
}
