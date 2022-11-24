using System;
using System.Collections.Generic;
using System.Text;

namespace SilverSoft.Utils.HttpHelper
{
    public class HttpClientConfigOptions
    {
        public string Name { get; set; }

        public string BaseAddress { get; set; }

        public string Path { get; set; }

        public string AcceptType { get; set; }

        public string Referer { get; set; }

        public string CacheControl { get; set; }

        public int Retry { get; set; }

        public int Timeout { get; set; }

        public IList<ApiInfo> Apis { get; set; }

        public HttpClientConfigOptions WithBaseAddress(string baseAddress)
        {
            BaseAddress = baseAddress;
            return this;
        }

        public HttpClientConfigOptions WithPath(string path)
        {
            Path = path;
            return this;
        }

        public HttpClientConfigOptions WithAcceptType(string acceptType)
        {
            AcceptType = acceptType;
            return this;
        }

        public HttpClientConfigOptions WithReferer(string referer)
        {
            Referer = referer;
            return this;
        }

        public HttpClientConfigOptions WithCacheControl(string cacheControl)
        {
            CacheControl = cacheControl;
            return this;
        }
    }

    public class ApiInfo
    {
        public string ApiName { get; set; }

        public string Path { get; set; }
    }
}
