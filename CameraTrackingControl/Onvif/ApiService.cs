using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CameraTrackingControl.Onvif
{


    //public class PostmanCollection
    //{
    //    public List<ApiRequest> Item { get; set; }
    //}

    //public class ApiRequest
    //{
    //    public string Name { get; set; }
    //    public ApiRequestDetails Request { get; set; }
    //    public List<object> Response { get; set; }
    //}

    //public class ApiRequestDetails
    //{
    //    public string Method { get; set; }
    //    public List<ApiHeader> Header { get; set; }
    //    public ApiUrl Url { get; set; }
    //}

    //public class ApiHeader
    //{
    //    public string Key { get; set; }
    //    public object Value { get; set; }
    //}

    //public class ApiUrl
    //{
    //    public string Raw { get; set; }
    //    public string Protocol { get; set; }
    //    public List<string> Host { get; set; }
    //    public string Port { get; set; }
    //    public List<string> Path { get; set; }
    //    public List<ApiQueryParam> Query { get; set; }
    //}

    //public class ApiQueryParam
    //{
    //    public string Key { get; set; }
    //    public object Value { get; set; }
    //}

    //public class ApiResponse
    //{
    //    public bool IsSuccess { get; set; }
    //    public string Content { get; set; }
    //    public int StatusCode { get; set; }
    //}


    public class ApiService
    {
        private readonly HttpClient client;
        private readonly string baseUri;
        private readonly string host;
        private readonly string username;
        private readonly string password;
        private readonly string profileToken;

        private readonly string postmanCollectionFilePath;
        private PostmanCollection postmanCollection;

        public ApiService(string baseUri, string host, string username, string password, string profileToken, string postmanCollectionFilePath)
        {
            this.client = new HttpClient();
            this.baseUri = baseUri;
            this.host = host;
            this.username = username;
            this.password = password;
            this.profileToken = profileToken;
            this.postmanCollectionFilePath = postmanCollectionFilePath;
        }

        private ApiRequest GetApiRequest(string itemName)
        {
            // 根据 itemName 获取相应的 ApiRequest 对象的逻辑
            // 你需要实现这个方法，根据 itemName 从 JSON 文件中找到对应的项
            // 然后将其转换为 ApiRequest 对象返回

            // 读取 JSON 文件内容
            // 如果 postmanCollection 为空，进行一次反序列化
            if (postmanCollection == null)
            {
                // 读取 JSON 文件内容
                string json = File.ReadAllText(postmanCollectionFilePath);

                // 反序列化为 PostmanCollection 对象
                postmanCollection = JsonHelper.Deserialize<PostmanCollection>(json);
            }

            // 根据 itemName 查找对应的 ApiRequest
            ApiRequest apiRequest = postmanCollection?.Item?.Find(request => request.Name == itemName);

            return apiRequest;
        }

        public async Task DiscoveryDevice()
        {
            var item = GetApiRequest("1.DiscoveryDevice");
            await ExecuteRequest(item);
        }

        public async Task GetCapabilities()
        {
            var parameters = new Dictionary<string, string>
        {
            { "host", host },
            { "username", username },
            { "password", password }
        };
            var item = GetApiRequest("2.GetCapabilities");
            await ExecuteRequest(item, parameters);
        }

        public async Task GetProfiles()
        {
            var parameters = new Dictionary<string, string>
        {
            { "host", host },
            { "username", username },
            { "password", password }
        };
            var item = GetApiRequest("3.GetProfiles");
            await ExecuteRequest(item, parameters);
        }

        public async Task GetVideoSources()
        {
            var parameters = new Dictionary<string, string>
        {
            { "host", host },
            { "username", username },
            { "password", password },
            { "profileToken", profileToken }
        };
            var item = GetApiRequest("4.GetVideoSources");
            await ExecuteRequest(item, parameters);
        }

        public async Task<PTZStatus> GetPTZStatus()
        {
            var parameters = new Dictionary<string, string>
        {
            { "host", host },
            { "username", username },
            { "password", password },
            { "profileToken", profileToken }
        };
            var item = GetApiRequest("5.GetPTZStatus");

            // 执行请求
            string responseJson = await ExecuteRequest(item, parameters);

            // 解析JSON字符串为PTZStatus对象
            PTZStatus ptzStatus = JsonHelper.Deserialize<PTZStatus>(responseJson);

            return ptzStatus;
        }

        public async Task PTZAbsoluteMove(double pan, double tilt, double zoom)
        {
            var parameters = new Dictionary<string, string>
            {
                { "host", host },
                { "username", username },
                { "password", password },
                { "profileToken", profileToken },
                { "pan", pan.ToString() },
                { "tilt", tilt.ToString() },
                { "zoom", zoom.ToString() },
                { "panSpeed", "1" },
                { "tiltSpeed", "1" },
                { "zoomSpeed", "1" }
            };
            var item = GetApiRequest("6.PTZAbsoluteMove");
            await ExecuteRequest(item, parameters);
        }

        public async Task PTZRelativeMove(double pan, double tilt, double zoom)
        {
            var parameters = new Dictionary<string, string>
        {
            { "host", host },
            { "username", username },
            { "password", password },
            { "profileToken", profileToken },
            { "pan", pan.ToString() },
            { "tilt", tilt.ToString() },
            { "zoom", zoom.ToString() },
            { "panSpeed", "1" },
            { "tiltSpeed", "1" },
            { "zoomSpeed", "1" }
        };
            var item = GetApiRequest("7.PTZRelativeMove");
            await ExecuteRequest(item, parameters);
        }

        public async Task GetPresets()
        {
            var parameters = new Dictionary<string, string>
        {
            { "host", host },
            { "username", username },
            { "password", password },
            { "profileToken", profileToken }
        };
            var item = GetApiRequest("8.GetPresets");
            await ExecuteRequest(item, parameters);
        }

        public async Task GotoPreset()
        {
            var parameters = new Dictionary<string, string>
        {
            { "host", host },
            { "username", username },
            { "password", password },
            { "profileToken", profileToken },
            { "presetToken", "4" },
            { "panSpeed", "1" },
            { "tiltSpeed", "1" },
            { "zoomSpeed", "1" }
        };
            var item = GetApiRequest("9.GotoPreset");
            await ExecuteRequest(item, parameters);
        }

        public async Task SetPreset()
        {
            var parameters = new Dictionary<string, string>
        {
            { "host", host },
            { "username", username },
            { "password", password },
            { "profileToken", profileToken },
            { "presetToken", "4" },
            { "presetName", "New1" }
        };
            var item = GetApiRequest("10.SetPreset");
            await ExecuteRequest(item, parameters);
        }

        private async Task<string> ExecuteRequest(ApiRequest item, Dictionary<string, string> parameters = null)
        {
            string content = string.Empty;
            if (item == null)
            {
                Console.WriteLine("ApiRequest is null.");
                return content;
            }

            // 根据请求方法调用相应的函数
            switch (item.Request.Method.ToUpper())
            {
                case "GET":
                    content = await ExecuteGetRequest(item, parameters);
                    break;
                case "POST":
                    content = await ExecutePostRequest(item, parameters);
                    break;
                default:
                    Console.WriteLine($"Unsupported request method: {item.Request.Method}");
                    break;
            }
            return content;
        }

        private async Task<string> ExecuteGetRequest(ApiRequest item, Dictionary<string, string> parameters = null)
        {
            // 构建请求消息
            var request = CreateHttpRequestMessage(item, parameters);

            // 执行请求
            var response = await client.SendAsync(request);

            // 读取响应内容
            string content = await response.Content.ReadAsStringAsync();

            // 处理响应...
            Console.WriteLine($"GET Request '{item.Name}' Response: {content}");
            return content;
        }

        private async Task<string> ExecutePostRequest(ApiRequest item, Dictionary<string, string> parameters = null)
        {
            // 构建请求消息
            var request = CreateHttpRequestMessage(item, parameters);

            // 执行请求
            var response = await client.SendAsync(request);

            // 读取响应内容
            string content = await response.Content.ReadAsStringAsync();

            // 处理响应...
            Console.WriteLine($"POST Request '{item.Name}' Response: {content}");

            return content;
        }

        private HttpRequestMessage CreateHttpRequestMessage(ApiRequest item, Dictionary<string, string> parameters = null)
        {
            string apiUrl = BuildFullUrl(item.Request.Url.Raw, parameters);

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = new HttpMethod(item.Request.Method),
                RequestUri = new Uri(apiUrl)
            };

            // 添加请求头
            foreach (var header in item.Request.Header)
            {
                httpRequestMessage.Headers.Add(header.Key, header.Value.ToString());
            }

            return httpRequestMessage;
        }

        private string BuildFullUrl(string apiUrl, Dictionary<string, string> queryParams)
        {
            // 构建 URL
            StringBuilder urlBuilder = new StringBuilder(apiUrl);

            // 添加查询参数（如果有）
            if (queryParams != null && queryParams.Count > 0)
            {
                urlBuilder.Append("?");
                urlBuilder.Append(string.Join("&", queryParams.Select(q => $"{Uri.EscapeDataString(q.Key)}={Uri.EscapeDataString(q.Value)}")));
            }

            return urlBuilder.ToString();
        }
    }


}
