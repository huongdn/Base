using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum NetworkConnectionState
{
    Disconnected,
    Connecting,
    Connected
}

public class NetworkClient : MonoBehaviour
{
    [SerializeField] private string serverUrl = "ws://localhost:3000";
    [SerializeField] private bool connectOnStart;

    public string ServerUrl => serverUrl;
    public NetworkConnectionState State { get; private set; } = NetworkConnectionState.Disconnected;
    public string RoomId { get; private set; }
    public string SessionToken { get; private set; }
    public string YourPlayer { get; private set; }

    public event Action OnConnected;
    public event Action<string> OnDisconnected;
    public event Action<NetworkConnectionState> OnStateChanged;
    public event Action<RoomJoinedMessage> OnRoomJoined;
    public event Action<GameStateMessage> OnGameState;
    public event Action<GameOverMessage> OnGameOver;
    public event Action<PlayerLeftMessage> OnPlayerLeft;
    public event Action<ErrorMessage> OnServerError;
    public event Action<string> OnUnknownMessage;

    private ClientWebSocket _webSocket;
    private CancellationTokenSource _cts;
    private readonly ConcurrentQueue<Action> _mainThreadActions = new();
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    private void Start()
    {
        if (connectOnStart)
            Connect();
    }

    private void Update()
    {
        while (_mainThreadActions.TryDequeue(out var action))
            action.Invoke();
    }

    private void OnDestroy()
    {
        _ = DisconnectInternalAsync(silent: true);
    }

    private void OnApplicationQuit()
    {
        _ = DisconnectInternalAsync(silent: true);
    }

    public void SetServerUrl(string url)
    {
        if (State == NetworkConnectionState.Disconnected)
            serverUrl = url;
    }

    public void Connect()
    {
        if (State != NetworkConnectionState.Disconnected) return;
        _ = ConnectInternalAsync();
    }

    public void Disconnect()
    {
        _ = DisconnectInternalAsync();
    }

    public void JoinRoom(string roomId = null)
    {
        if (State != NetworkConnectionState.Connected) return;
        _ = SendTextAsync(MessageSerializer.SerializeJoinRoom(roomId));
    }

    public void MakeMove(int cellIndex)
    {
        if (State != NetworkConnectionState.Connected) return;
        _ = SendTextAsync(MessageSerializer.SerializeMakeMove(cellIndex));
    }

    public void RejoinRoom(string roomId, string sessionToken)
    {
        if (State != NetworkConnectionState.Connected) return;
        _ = SendTextAsync(MessageSerializer.SerializeRejoinRoom(roomId, sessionToken));
    }

    private async Task ConnectInternalAsync()
    {
        SetState(NetworkConnectionState.Connecting);

        _cts = new CancellationTokenSource();
        _webSocket = new ClientWebSocket();

        try
        {
            var uri = new Uri(serverUrl);
            await _webSocket.ConnectAsync(uri, _cts.Token);
            _ = ReceiveLoopAsync(_cts.Token);

            EnqueueMainThread(() =>
            {
                SetState(NetworkConnectionState.Connected);
                OnConnected?.Invoke();
            });
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[NetworkClient] Connect failed: {ex.Message}");
            await DisconnectInternalAsync(silent: true);
            EnqueueMainThread(() => OnDisconnected?.Invoke(ex.Message));
        }
    }

    private async Task DisconnectInternalAsync(bool silent = false)
    {
        var previousState = State;
        SetState(NetworkConnectionState.Disconnected);

        _cts?.Cancel();

        if (_webSocket != null)
        {
            try
            {
                if (_webSocket.State == WebSocketState.Open)
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NetworkClient] Close failed: {ex.Message}");
            }

            _webSocket.Dispose();
            _webSocket = null;
        }

        _cts?.Dispose();
        _cts = null;

        if (!silent && previousState != NetworkConnectionState.Disconnected)
            EnqueueMainThread(() => OnDisconnected?.Invoke("Disconnected"));
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        var buffer = new byte[8192];

        try
        {
            while (!token.IsCancellationRequested && _webSocket?.State == WebSocketState.Open)
            {
                var builder = new StringBuilder();
                WebSocketReceiveResult result;

                do
                {
                    result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectInternalAsync();
                        return;
                    }

                    builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                } while (!result.EndOfMessage);

                var json = builder.ToString();
                EnqueueMainThread(() => HandleServerMessage(json));
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during disconnect.
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[NetworkClient] Receive failed: {ex.Message}");
            await DisconnectInternalAsync();
            EnqueueMainThread(() => OnDisconnected?.Invoke(ex.Message));
        }
    }

    private async Task SendTextAsync(string json)
    {
        if (_webSocket == null || _webSocket.State != WebSocketState.Open) return;

        var bytes = Encoding.UTF8.GetBytes(json);

        await _sendLock.WaitAsync();
        try
        {
            await _webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                _cts?.Token ?? CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[NetworkClient] Send failed: {ex.Message}");
            await DisconnectInternalAsync();
            EnqueueMainThread(() => OnDisconnected?.Invoke(ex.Message));
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private void HandleServerMessage(string json)
    {
        MessageSerializer.Dispatch(
            json,
            onRoomJoined: msg =>
            {
                RoomId = msg.payload.roomId;
                SessionToken = msg.payload.sessionToken;
                YourPlayer = msg.payload.yourPlayer;
                OnRoomJoined?.Invoke(msg);
            },
            onGameState: msg => OnGameState?.Invoke(msg),
            onGameOver: msg => OnGameOver?.Invoke(msg),
            onPlayerLeft: msg => OnPlayerLeft?.Invoke(msg),
            onError: msg => OnServerError?.Invoke(msg),
            onUnknown: msg => OnUnknownMessage?.Invoke(msg));
    }

    private void SetState(NetworkConnectionState state)
    {
        if (State == state) return;
        State = state;
        EnqueueMainThread(() => OnStateChanged?.Invoke(state));
    }

    private void EnqueueMainThread(Action action)
    {
        _mainThreadActions.Enqueue(action);
    }
}
