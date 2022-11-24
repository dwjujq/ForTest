using Microsoft.Net.Http.Headers;
using System.Buffers;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Wangkanai.Detection.Services;

namespace SilverSoft.Utils.HttpHelper
{
    public class HttpFileDownload
    {
        private static readonly Dictionary<string, string> _extensionMediaTypeDic;

        private HttpContext _httpContext;

        private string _contentType;

        private string _charset;

        private string _cacheControl;

        private string _pragma;

        private IDictionary<string, string> _headers = null;

        private string _fileName;

        private string _filePath;

        private Stream _stream = null;

        static HttpFileDownload()
        {
            _extensionMediaTypeDic = new Dictionary<string, string>
            {
                {".xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                {".docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                {".pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                {".pdf","application/pdf" },
                {".jpg","image/jpeg" },
                {".jpeg","image/jpeg" },
                {".png","image/png" },
                {".xls","application/vnd.ms-excel" },
                {".doc","application/msword" },
                {".ppt","application/x-ppt" }
            };
        }

        public HttpFileDownload() { }

        public HttpFileDownload(HttpContext httpContext)
        {
            this._httpContext = httpContext;
        }

        public HttpFileDownload WithFileName(string fileName)
        {
            this._fileName = fileName;
            return this;
        }

        public HttpFileDownload WithFilePath(string filePath)
        {
            this._filePath = filePath;
            return this;
        }

        public HttpFileDownload WithContentType(string contentType)
        {
            this._contentType = contentType;
            return this;
        }

        public HttpFileDownload WithCharset(string charset)
        {
            this._charset = charset;
            return this;
        }

        public HttpFileDownload WithCacheControl(string cacheControl)
        {
            this._cacheControl = cacheControl;
            return this;
        }

        public HttpFileDownload WithPragma(string pragma)
        {
            this._pragma = pragma;
            return this;
        }

        public HttpFileDownload WithHeaders(IDictionary<string, string> headers)
        {
            this._headers = headers;
            return this;
        }

        public HttpFileDownload WithStream(Stream stream)
        {
            this._stream = stream;
            return this;
        }

        public HttpFileDownload WithStream(Func<Stream> func)
        {
            this._stream = func();
            return this;
        }

        public Stream GetFileStream()
        {
            if (_filePath.StartsWith("http://") || _filePath.StartsWith("https://"))
            {
                var httpClientConfig = HttpClientCommon.HttpClientConfigOptionsList.FirstOrDefault(o => o.Name == "Download");
                httpClientConfig.WithPath(_filePath);
                return HttpClientHelper.GetAsync<Stream>(httpClientConfig).Result;
            }

            if (!File.Exists(_filePath))
            {
                throw new Exception($"文件\"{_filePath}\"不存在");
            }

            return new FileStream(_filePath, FileMode.Open, FileAccess.Read);
        }

        private Stream GetStream()
        {
            if (_stream == null)
            {
                _stream = GetFileStream();
            }
            if (_stream.CanSeek)
            {
                _stream.Seek(0, SeekOrigin.Begin);
                return _stream;
            }

            try
            {
                var contentLength = _stream.Length;
            }
            catch
            {
                var memoryStream = new MemoryStream();
                _stream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            finally
            {
                if (_stream.CanSeek)
                {
                    _stream.Seek(0, SeekOrigin.Begin);
                }
            }
            return _stream;
        }

        public void Send()
        {
            if (_httpContext == null)
            {
                throw new Exception($"{nameof(_httpContext)}不能为空");
            }

            if (string.IsNullOrWhiteSpace(_fileName))
            {
                throw new Exception($"{nameof(_fileName)}不能为空");
            }

            if (string.IsNullOrWhiteSpace(_contentType))
            {
                if (_headers != null && _headers.TryGetValue("Content-Type", out var contentType))
                {
                    _contentType = contentType;
                    _headers.Remove("Content-Type");
                }
                else
                {
                    var ext = Path.GetExtension(_fileName).ToLowerInvariant();
                    if (_extensionMediaTypeDic.TryGetValue(ext, out var mediaType))
                    {
                        _contentType = mediaType;
                    }
                    else
                    {
                        _contentType = "application/octet-stream";
                    }
                }
            }

            if (_contentType.IndexOf("chaset", StringComparison.InvariantCultureIgnoreCase) == -1 && !string.IsNullOrWhiteSpace(_charset))
            {
                _contentType = $"{_contentType};charset={_charset}";
            }
            _httpContext.Response.ContentType = _contentType;

            if (!string.IsNullOrWhiteSpace(_cacheControl))
            {
                _httpContext.Response.Headers.CacheControl = _cacheControl;
            }
            else if (_headers != null && _headers.TryGetValue("Cache-Control", out var cacheControl))
            {
                _httpContext.Response.Headers.CacheControl=cacheControl;
                _headers.Remove("Cache-Control");
            }

            if (!string.IsNullOrWhiteSpace(_pragma))
            {
                _httpContext.Response.Headers.Pragma= _pragma;
            }
            else if (_headers != null && _headers.TryGetValue("Pragma", out var pragma))
            {
                _httpContext.Response.Headers.Pragma = pragma;
                _headers.Remove("Pragma");
            }

            if (_headers?.Count > 0)
            {
                foreach (var header in _headers)
                {
                    _httpContext.Response.Headers.TryAdd(header.Key, header.Value);
                }
            }

            var stream = GetStream();
            try
            {
                _httpContext.Response.ContentLength = stream.Length;

                var browserType = _httpContext.RequestServices.GetRequiredService<IDetectionService>().Browser.Name.ToString();
                if (browserType == "Chrome")
                {
                    using (_httpContext.Response.Body)
                    {
                        _fileName = HttpUtility.UrlEncode(_fileName);
                        //以字符流的形式下载文件，谷歌浏览器不支持分块下载
                        _httpContext.Response.Headers.Add("Content-Disposition", $"attachment; filename={_fileName.Replace("+", "%20")}");
                        stream.CopyTo(_httpContext.Response.Body);
                        _httpContext.Response.Body.Flush();
                    }
                }
                else
                {
                    if (browserType == "Firefox")
                    {
                        _httpContext.Response.Headers.Add("Content-Disposition", "attachment;filename=\"=?UTF-8?B?" + Convert.ToBase64String(Encoding.UTF8.GetBytes(_fileName)) + "?=\"");
                    }
                    else
                    {
                        _fileName = HttpUtility.UrlEncode(_fileName);
                        _httpContext.Response.Headers.Add("Content-Disposition", $"attachment;filename={_fileName.Replace("+", "%20")}");
                    }

                    var bufferSize = 102400;
                    using (_httpContext.Response.Body)
                    {
                        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                        try
                        {
                            int bytesRead;
                            while ((bytesRead = _stream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                _httpContext.Response.Body.Write(buffer, 0, bytesRead);
                                _httpContext.Response.Body.Flush();
                            }
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(buffer);
                        }
                    }
                }
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }
        }
    }
}
