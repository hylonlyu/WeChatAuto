using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WeChatAuto
{
    /// <summary>
    /// HTTP消息客户端，用于从HTTP消息服务器轮询消息
    /// </summary>
    public class HttpMessageClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;
        private bool _isPolling;
        private int _pollInterval = 1000; // 1秒轮询间隔

        public bool IsPolling { get { return _isPolling; } }

        // 事件通知
        public event Action<string> MessageReceived;
        public event Action<string> ClientLog;
        public event Action<string> ClientError;

        public HttpMessageClient(string serverIp, int port)
        {
            _serverUrl = "http://" + serverIp + ":" + port.ToString();
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10); // 10秒超时
        }

        public void StartPolling()
        {
            if (_isPolling) return;

            _isPolling = true;
            if (ClientLog != null)
            {
                ClientLog("开始轮询HTTP消息服务器: " + _serverUrl);
            }

            Task.Run(() => PollingLoopAsync());
        }

        public void StopPolling()
        {
            _isPolling = false;
            if (ClientLog != null)
            {
                ClientLog("停止轮询HTTP消息服务器");
            }
        }

        /// <summary>
        /// 轮询循环
        /// </summary>
        private async Task PollingLoopAsync()
        {
            while (_isPolling)
            {
                try
                {
                    // 检查是否有新消息
                    string message = await CheckForMessagesAsync();

                    if (!string.IsNullOrEmpty(message) && message != "NO_MESSAGE")
                    {
                        if (MessageReceived != null)
                        {
                            MessageReceived(message);
                        }
                    }

                    // 等待下次轮询
                    await Task.Delay(_pollInterval);
                }
                catch (Exception ex)
                {
                    if (ClientError != null)
                    {
                        ClientError("轮询时出错: " + ex.Message);
                    }

                    // 出错时等待较长时间后重试
                    await Task.Delay(5000);
                }
            }
        }

        /// <summary>
        /// 检查是否有新消息
        /// </summary>
        private async Task<string> CheckForMessagesAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(_serverUrl + "/check");
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (HttpRequestException ex)
            {
                if (ClientError != null)
                {
                    ClientError("HTTP请求失败: " + ex.Message);
                }
                return null;
            }
            catch (TaskCanceledException)
            {
                // 请求超时，正常情况
                return null;
            }
        }

        /// <summary>
        /// 检查服务器状态
        /// </summary>
        public async Task<bool> CheckServerStatusAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(_serverUrl + "/status");
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();

                // 解析状态: OK|客户端数量|消息队列数量
                string[] parts = content.Split('|');
                if (parts.Length >= 1 && parts[0] == "OK")
                {
                    if (ClientLog != null)
                    {
                        ClientLog("服务器状态正常: " + content);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                if (ClientError != null)
                {
                    ClientError("检查服务器状态失败: " + ex.Message);
                }
                return false;
            }
        }

        /// <summary>
        /// 设置轮询间隔（毫秒）
        /// </summary>
        public void SetPollInterval(int milliseconds)
        {
            if (milliseconds >= 500 && milliseconds <= 60000) // 限制在0.5秒到60秒之间
            {
                _pollInterval = milliseconds;
                if (ClientLog != null)
                {
                    ClientLog("轮询间隔已设置为: " + milliseconds.ToString() + "毫秒");
                }
            }
        }

        public void Dispose()
        {
            StopPolling();
            _httpClient?.Dispose();
        }
    }
}