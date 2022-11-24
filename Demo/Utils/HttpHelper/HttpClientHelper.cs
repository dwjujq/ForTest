using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace SilverSoft.Utils.HttpHelper
{
    public enum AcceptTypeEnum
    {
        Json,
        Xml
    }

    public class HttpClientHelper
    {
        #region 用HttpClientFactory创建HttpClient，推荐这种方式

        /// <summary>
        /// Get请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configOptions"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<T> GetAsync<T>(HttpClientConfigOptions configOptions)
        {
            var client = HttpClientCommon.HttpClientFactory.CreateClient(configOptions.Name);
            var request = new HttpRequestMessage(HttpMethod.Get,configOptions.Path);

            client.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");

            if (!string.IsNullOrWhiteSpace(configOptions.AcceptType))
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(configOptions.AcceptType));
            }

            if (!string.IsNullOrWhiteSpace(configOptions.Referer))
            {
                client.DefaultRequestHeaders.Referrer = new Uri(configOptions.Referer);
            }

            if (!string.IsNullOrWhiteSpace(configOptions.CacheControl) && CacheControlHeaderValue.TryParse(configOptions.CacheControl, out var cacheControlHeader))
            {
                client.DefaultRequestHeaders.CacheControl = cacheControlHeader;
            }

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("请求失败");
            }

            if (typeof(T) == typeof(Stream))
            {
                return (T)(object)response.Content.ReadAsStream();
            }

            var responseString = await response.Content.ReadAsStringAsync();

            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(responseString, typeof(T));
            }

            return JsonConvert.DeserializeObject<T>(responseString);
        }

        /// <summary>
        /// Get请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="clientName"></param>
        /// <param name="path"></param>
        /// <param name="queryParams"></param>
        /// <param name="addReferer"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(string clientName, string path=null, string queryParams = null, bool addReferer = false)
        {
            var clientConfigOptions = HttpClientCommon.HttpClientConfigOptionsList.FirstOrDefault(c => c.Name == clientName);

            var pathAndQuery = path ?? string.Empty;
            #region
            if (!string.IsNullOrWhiteSpace(queryParams))
            {
                pathAndQuery = $"{(pathAndQuery.Contains("?") ? pathAndQuery : $"{pathAndQuery}?")}{queryParams}";
            }
            #endregion

            var client = HttpClientCommon.HttpClientFactory.CreateClient(clientName);
            var request = new HttpRequestMessage(HttpMethod.Get, pathAndQuery);

            client.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");

            if (!string.IsNullOrWhiteSpace(clientConfigOptions.AcceptType))
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(clientConfigOptions.AcceptType));
            }
            if (addReferer)
            {
                if (!string.IsNullOrWhiteSpace(clientConfigOptions.Referer))
                {
                    client.DefaultRequestHeaders.Referrer = new Uri(clientConfigOptions.BaseAddress);
                }
                else { client.DefaultRequestHeaders.Referrer = new Uri(clientConfigOptions.Referer); }
            }
            if (!string.IsNullOrWhiteSpace(clientConfigOptions.CacheControl) && CacheControlHeaderValue.TryParse(clientConfigOptions.CacheControl, out var cacheControlHeader))
            {
                client.DefaultRequestHeaders.CacheControl = cacheControlHeader;
            }

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("请求失败");
            }

            if (typeof(T) == typeof(Stream))
            {
                return (T)Convert.ChangeType(response.Content.ReadAsStream(), typeof(T));
            }

            var responseString = await response.Content.ReadAsStringAsync();

            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(responseString, typeof(T));
            }

            return JsonConvert.DeserializeObject<T>(responseString);
        }

        /// <summary>
        /// Get请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="clientName"></param>
        /// <param name="apiName"></param>
        /// <param name="queryParams"></param>
        /// <param name="addReferer"></param>
        /// <returns></returns>
        public static async Task<T> GetApiAsync<T>(string clientName, string apiName, string queryParams = null, bool addReferer = false)
        {
            var clientConfigOptions = HttpClientCommon.HttpClientConfigOptionsList.FirstOrDefault(c => c.Name == clientName);
            var apiConfig = clientConfigOptions.Apis.FirstOrDefault(a => a.ApiName == apiName);

            return await GetAsync<T>(clientName, apiConfig?.Path, queryParams, addReferer);
        }

        /// <summary>
        /// Get请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="clientName"></param>
        /// <param name="apiName"></param>
        /// <param name="queryDic"></param>
        /// <param name="addReferer"></param>
        /// <returns></returns>
        public static async Task<T> GetApiAsync<T>(string clientName, string apiName=null, Dictionary<string, string> queryDic = null, bool addReferer = false)
        {
            #region
            var urlParams = new StringBuilder();
            if (queryDic != null && queryDic.Count > 0)
            {
                foreach (var item in queryDic)
                {
                    urlParams.Append($"{item.Key}={item.Value}&");
                }
            }
            if (urlParams.Length > 0)
            {
                urlParams = urlParams.Remove(urlParams.Length - 1, 1);
            }
            #endregion

            return await GetAsync<T>(clientName, apiName, urlParams.ToString(), addReferer);
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="clientName"></param>
        /// <param name="apiName"></param>
        /// <param name="postData"></param>
        /// <param name="queryParams"></param>
        /// <param name="addReferer"></param>
        /// <returns></returns>
        public static async Task<T> PostAsync<T>(string clientName, string apiName, dynamic postData, string queryParams = null, bool addReferer = false)
        {

            var clientConfigOptions = HttpClientCommon.HttpClientConfigOptionsList.FirstOrDefault(c => c.Name == clientName);
            var apiConfig = clientConfigOptions.Apis.FirstOrDefault(a => a.ApiName == apiName);

            var pathAndQuery = apiConfig?.Path??string.Empty;
            #region
            if (!string.IsNullOrWhiteSpace(queryParams))
            {
                pathAndQuery = $"{(pathAndQuery.Contains("?") ? pathAndQuery : $"{pathAndQuery}?")}{queryParams}";
            }
            #endregion

            var client = HttpClientCommon.HttpClientFactory.CreateClient(clientName);
            var request = new HttpRequestMessage(HttpMethod.Post, pathAndQuery);

            client.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(clientConfigOptions.AcceptType));
            if (addReferer)
            {
                if (!string.IsNullOrWhiteSpace(clientConfigOptions.Referer))
                {
                    client.DefaultRequestHeaders.Referrer = new Uri(clientConfigOptions.BaseAddress);
                }
                else { client.DefaultRequestHeaders.Referrer = new Uri(clientConfigOptions.Referer); }
            }

            var httpContent = new StringContent(JsonConvert.SerializeObject(postData));
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = httpContent;

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("请求失败");
            }

            var responseString = await response.Content.ReadAsStringAsync();

            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(responseString, typeof(T));
            }

            return JsonConvert.DeserializeObject<T>(responseString);
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="clientName"></param>
        /// <param name="apiName"></param>
        /// <param name="postData"></param>
        /// <param name="queryDic"></param>
        /// <param name="addReferer"></param>
        /// <returns></returns>
        public static async Task<T> PostAsync<T>(string clientName, string apiName, dynamic postData, Dictionary<string, string> queryDic = null, bool addReferer = false)
        {
            #region
            var urlParams = new StringBuilder();
            if (queryDic != null && queryDic.Count > 0)
            {
                foreach (var item in queryDic)
                {
                    urlParams.Append($"{item.Key}={item.Value}&");
                }
            }
            if (urlParams.Length > 0)
            {
                urlParams = urlParams.Remove(urlParams.Length - 1, 1);
            }
            #endregion

            return await PostAsync<T>(clientName, apiName,postData, urlParams.ToString(), addReferer);
        }

        #endregion

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="queryDic"></param>
        /// <param name="acceptType"></param>
        /// <param name="referer"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(string url, Dictionary<string, string> queryDic = null, string acceptType = "application/json",string referer=null)
        {
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.None };
            using (var httpClient = new HttpClient(handler))
            {
                #region
                var urlParams = new StringBuilder();
                if (queryDic != null && queryDic.Count > 0)
                {
                    foreach (var item in queryDic)
                    {
                        urlParams.Append($"{item.Key}={item.Value}&");
                    }
                }
                if (urlParams.Length > 0)
                {
                    urlParams = urlParams.Remove(urlParams.Length - 1, 1);
                    url = $"{url}?{urlParams.ToString()}";
                }
                #endregion

                httpClient.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptType));
                if (!string.IsNullOrWhiteSpace(referer))
                {
                    httpClient.DefaultRequestHeaders.Referrer = new Uri(referer);
                }
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("请求失败");
                }

                var responseString = await response.Content.ReadAsStringAsync();

                if(typeof(T)==typeof(string))
                {
                    return (T)Convert.ChangeType(responseString, typeof(T));
                }

                return JsonConvert.DeserializeObject<T>(responseString);
            }
        }


        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="queryDic"></param>
        /// <param name="acceptType"></param>
        /// <param name="referer"></param>
        /// <returns></returns>
        public static async Task<T> PostAsync<T>(string url,dynamic postData, Dictionary<string, string> queryDic = null, string acceptType = "application/json", string referer = null)
        {
            try
            {
                var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.None };
                using (var httpClient = new HttpClient(handler))
                {
                    #region
                    var urlParams = new StringBuilder();
                    if (queryDic != null && queryDic.Count > 0)
                    {
                        foreach (var item in queryDic)
                        {
                            urlParams.Append($"{item.Key}={item.Value}&");
                        }
                    }
                    if (urlParams.Length > 0)
                    {
                        urlParams = urlParams.Remove(urlParams.Length - 1, 1);
                        url = $"{url}?{urlParams.ToString()}";
                    }
                    #endregion
                    httpClient.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptType));
                    var httpContent = new StringContent(JsonConvert.SerializeObject(postData));
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    if (!string.IsNullOrWhiteSpace(referer))
                    {
                        httpClient.DefaultRequestHeaders.Referrer = new Uri(referer);
                    }

                    var response = await httpClient.PostAsync(url, httpContent);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("请求失败");
                    }

                    var responseString = await response.Content.ReadAsStringAsync();

                    if (typeof(T) == typeof(string))
                    {
                        return (T)Convert.ChangeType(responseString, typeof(T));
                    }

                    var domain= JsonConvert.DeserializeObject<T>(responseString);
                    return domain;
                }
            }
            catch (Exception e)
            {

                throw;
            }

        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="fileStream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<T> UploadAsync<T>(string url, Stream fileStream, string fileName)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "CBS Brightcove API Service");

                using (var content = new MultipartFormDataContent())
                {
                    var streamContent = new StreamContent(fileStream);
                    streamContent.Headers.Add("Content-Type", "application/octet-stream");
                    //Content-Disposition: form-data; name="file"; filename="C:\B2BAssetRoot\files\596090\596090.1.mp4";
                    streamContent.Headers.Add("Content-Disposition", "form-data; name=\"file\"; filename=\"" + WebUtility.UrlEncode(fileName) + "\"");
                    content.Add(streamContent, "file", WebUtility.UrlEncode(fileName));

                    //content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");

                    var response =await client.PostAsync(url, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("请求失败");
                    }

                    var responseString = await response.Content.ReadAsStringAsync();

                    if (typeof(T) == typeof(string))
                    {
                        return (T)Convert.ChangeType(responseString, typeof(T));
                    }

                    return JsonConvert.DeserializeObject<T>(responseString);
                }
            }
        }
    }
}
