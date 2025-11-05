using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace ValleyBot.Service;

public class AdvancedWebSocketClient : IDisposable
{
    private ClientWebSocket _webSocket;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly Uri _serverUri;
    private Task _receiveTask;
    private readonly JsonSerializerOptions _jsonOptions;

    // 事件定义
    public event EventHandler Connected;
    public event EventHandler<string> MessageReceived;
    public event EventHandler<byte[]> BinaryMessageReceived;
    public event EventHandler<JsonElement> JsonMessageReceived;  // 新增：JSON 消息事件
    public event EventHandler<Exception> ErrorOccurred;
    public event EventHandler Disconnected;

    // 属性
    public bool IsConnected => _webSocket?.State == WebSocketState.Open;
    public WebSocketState State => _webSocket?.State ?? WebSocketState.None;

    public AdvancedWebSocketClient(string serverUrl, JsonSerializerOptions jsonOptions = null)
    {
        _serverUri = new Uri(serverUrl);
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    /// <summary>
    /// 连接到 WebSocket 服务器
    /// </summary>
    public async Task ConnectAsync(TimeSpan? timeout = null)
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket 已经连接");
        }

        _webSocket = new ClientWebSocket();
        _cancellationTokenSource = new CancellationTokenSource();

        ConfigureWebSocket(_webSocket);

        try
        {
            var connectTimeout = timeout ?? TimeSpan.FromSeconds(30);
            using (var timeoutCts = new CancellationTokenSource(connectTimeout))
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                timeoutCts.Token, _cancellationTokenSource.Token))
            {
                await _webSocket.ConnectAsync(_serverUri, linkedCts.Token);
            }

            _receiveTask = Task.Run(ReceiveLoopAsync);

            Connected?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
            throw;
        }
    }

    /// <summary>
    /// 配置 WebSocket 选项
    /// </summary>
    private void ConfigureWebSocket(ClientWebSocket webSocket)
    {
        webSocket.Options.SetRequestHeader("User-Agent", "MyWebSocketClient/1.0");
        webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
        webSocket.Options.SetBuffer(8192, 8192);
    }

    #region 发送方法

    /// <summary>
    /// 发送文本消息
    /// </summary>
    public async Task SendTextAsync(string message)
    {
        EnsureConnected();
        var bytes = Encoding.UTF8.GetBytes(message);
        await SendAsync(bytes, WebSocketMessageType.Text);
    }

    /// <summary>
    /// 发送二进制消息
    /// </summary>
    public async Task SendBinaryAsync(byte[] data)
    {
        EnsureConnected();
        await SendAsync(data, WebSocketMessageType.Binary);
    }

    /// <summary>
    /// 发送 JSON 对象（泛型版本）
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="data">要发送的对象</param>
    public async Task SendJsonAsync<T>(T data)
    {
        EnsureConnected();

        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            await SendTextAsync(json);
        }
        catch (JsonException ex)
        {
            var jsonEx = new InvalidOperationException($"JSON 序列化失败: {ex.Message}", ex);
            ErrorOccurred?.Invoke(this, jsonEx);
            throw jsonEx;
        }
    }

    /// <summary>
    /// 发送 JSON 字符串（已序列化的 JSON）
    /// </summary>
    public async Task SendJsonStringAsync(string jsonString)
    {
        EnsureConnected();

        // 验证是否为有效 JSON
        try
        {
            JsonDocument.Parse(jsonString);
        }
        catch (JsonException ex)
        {
            var jsonEx = new InvalidOperationException($"无效的 JSON 字符串: {ex.Message}", ex);
            ErrorOccurred?.Invoke(this, jsonEx);
            throw jsonEx;
        }

        await SendTextAsync(jsonString);
    }

    /// <summary>
    /// 发送匿名对象为 JSON
    /// </summary>
    public async Task SendJsonAsync(object anonymousObject)
    {
        await SendJsonAsync<object>(anonymousObject);
    }

    /// <summary>
    /// 批量发送 JSON 消息
    /// </summary>
    public async Task SendJsonBatchAsync<T>(IEnumerable<T> dataList, TimeSpan? delayBetweenMessages = null)
    {
        EnsureConnected();

        var delay = delayBetweenMessages ?? TimeSpan.FromMilliseconds(10);

        foreach (var data in dataList)
        {
            await SendJsonAsync(data);
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay);
            }
        }
    }

    #endregion

    /// <summary>
    /// 发送消息的核心方法
    /// </summary>
    private async Task SendAsync(byte[] data, WebSocketMessageType messageType)
    {
        try
        {
            await _webSocket.SendAsync(
                new ArraySegment<byte>(data),
                messageType,
                endOfMessage: true,
                _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
            throw;
        }
    }

    #region 接收方法

    /// <summary>
    /// 接收消息循环
    /// </summary>
    private async Task ReceiveLoopAsync()
    {
        var buffer = new byte[8192];
        var messageBuffer = new List<byte>();

        try
        {
            while (_webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;

                do
                {
                    result = await _webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        _cancellationTokenSource.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await HandleCloseAsync(result);
                        return;
                    }

                    messageBuffer.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));

                } while (!result.EndOfMessage);

                var completeMessage = messageBuffer.ToArray();
                messageBuffer.Clear();

                HandleReceivedMessage(completeMessage, result.MessageType);
            }
        }
        catch (OperationCanceledException)
        {
            // 正常取消操作
        }
        catch (WebSocketException wsEx)
        {
            ErrorOccurred?.Invoke(this, wsEx);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
        finally
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 处理接收到的消息
    /// </summary>
    private async Task HandleReceivedMessage(byte[] data, WebSocketMessageType messageType)
    {
        try
        {
            if (messageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(data);
                
                // 触发原始文本消息事件
                MessageReceived?.Invoke(this, message);

                // 尝试解析为 JSON 并触发 JSON 事件
                //TryParseAndInvokeJson(message);
            }
            else if (messageType == WebSocketMessageType.Binary)
            {
                BinaryMessageReceived?.Invoke(this, data);
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
    }

    /// <summary>
    /// 尝试将文本解析为 JSON
    /// </summary>
    private void TryParseAndInvokeJson(string message)
    {
        if (JsonMessageReceived == null) return;

        try
        {
            using var jsonDoc = JsonDocument.Parse(message);
            JsonMessageReceived?.Invoke(this, jsonDoc.RootElement.Clone());
        }
        catch (JsonException)
        {
            // 不是有效的 JSON，忽略
        }
    }

    /// <summary>
    /// 等待并接收单个 JSON 消息（带超时）
    /// </summary>
    public async Task<T> ReceiveJsonAsync<T>(TimeSpan? timeout = null)
    {
        var tcs = new TaskCompletionSource<T>();
        var timeoutDuration = timeout ?? TimeSpan.FromSeconds(30);

        EventHandler<string> handler = null;
        handler = (sender, message) =>
        {
            try
            {
                var result = JsonSerializer.Deserialize<T>(message, _jsonOptions);
                tcs.TrySetResult(result);
            }
            catch (JsonException ex)
            {
                tcs.TrySetException(ex);
            }
            finally
            {
                MessageReceived -= handler;
            }
        };

        MessageReceived += handler;

        using (var cts = new CancellationTokenSource(timeoutDuration))
        {
            cts.Token.Register(() =>
            {
                MessageReceived -= handler;
                tcs.TrySetException(new TimeoutException("接收 JSON 消息超时"));
            });

            return await tcs.Task;
        }
    }

    #endregion

    /// <summary>
    /// 处理关闭消息
    /// </summary>
    private async Task HandleCloseAsync(WebSocketReceiveResult result)
    {
        try
        {
            await _webSocket.CloseOutputAsync(
                result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                result.CloseStatusDescription,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
    }

    /// <summary>
    /// 关闭连接
    /// </summary>
    public async Task CloseAsync(
        WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure,
        string statusDescription = "Client closing")
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.CloseAsync(
                    closeStatus,
                    statusDescription,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
            }
        }

        _cancellationTokenSource?.Cancel();

        if (_receiveTask != null)
        {
            await _receiveTask;
        }
    }

    /// <summary>
    /// 确保已连接
    /// </summary>
    private void EnsureConnected()
    {
        if (_webSocket?.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket 未连接或连接已关闭");
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _webSocket?.Dispose();
    }
}
