using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;

namespace MatsueNet.Services
{
    public class HttpService : HttpClient
    {
        private const string ProxyUrl = "https://proxy.duckduckgo.com/iu/?u=";

        public HttpService()
        {
            this.Timeout = new TimeSpan(0, 0, 10);
            this.MaxResponseContentBufferSize = 8000000; // Limit for non-nitro users. (8mb)
        }

        public async Task<Image<T>> GetImageAsync<T>(string url) where T : unmanaged, IPixel<T>
        {
            if (url.Contains(".gif"))
            {
                return null;
            }

            try
            {
                using (HttpResponseMessage response = await this.GetAsync(ProxyUrl + Uri.EscapeDataString(url)))
                {
                    string mt = response.Content.Headers.ContentType.MediaType;

                    if (response.IsSuccessStatusCode && mt.Contains("image/") && !mt.Contains("gif"))
                    {
                        Image<T> image = Image.Load<T>(await response.Content.ReadAsByteArrayAsync());
                        image.Metadata.ExifProfile = null;
                        return image;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Image<T>> GetGifAsync<T>(string url) where T : unmanaged, IPixel<T>
        {
            try
            {
                using (HttpResponseMessage response = await this.GetAsync(ProxyUrl + Uri.EscapeDataString(url)))
                {
                    string mt = response.Content.Headers.ContentType.MediaType;

                    if (response.IsSuccessStatusCode && mt.Contains("image/gif"))
                    {
                        return Image.Load<T>(await response.Content.ReadAsByteArrayAsync());
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<JObject> GetJObjectAsync(string url)
        {
            try
            {
                return JObject.Parse(await this.GetStringAsync(url));
            }
            catch
            {
                return null;
            }
        }

        public async Task<JArray> GetJArrayAsync(string url)
        {
            try
            {
                return JArray.Parse(await GetStringAsync(url));
            }
            catch
            {
                return null;
            }
        }

        public async Task<T> GetJsonAsync<T>(string url)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(await GetStringAsync(url));
            }
            catch
            {
                return default(T);
            }
        }

        public async Task<T> GetWithHostAsync<T>(string uri, string host, string accept = "*/*")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Accept = accept;
            request.Host = host;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync());
                }
                catch
                {
                    return default(T);
                }
            }
        }

        public async Task<T> PostAsyncYes<T>(string uri, string host, string accept = "*/*")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Accept = accept;
            request.Host = host;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync());
                }
                catch
                {
                    return default(T);
                }
            }
        }
    }
}