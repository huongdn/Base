using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(NetworkClient))]
public class NetworkUI : MonoBehaviour
{
    private NetworkClient _client;

    private TextField _serverUrlField;
    private TextField _roomIdField;
    private Button _connectBtn;
    private Button _joinBtn;
    private Label _connectionLabel;

    private bool _joinAfterConnect;
    private string _pendingRoomId;

    private void Awake()
    {
        _client = GetComponent<NetworkClient>();
    }

    private void OnEnable()
    {
        _client.OnConnected += HandleConnected;
        _client.OnStateChanged += HandleStateChanged;
        _client.OnRoomJoined += HandleRoomJoined;
        _client.OnServerError += HandleServerError;
        _client.OnDisconnected += HandleDisconnected;
    }

    private void OnDisable()
    {
        _client.OnConnected -= HandleConnected;
        _client.OnStateChanged -= HandleStateChanged;
        _client.OnRoomJoined -= HandleRoomJoined;
        _client.OnServerError -= HandleServerError;
        _client.OnDisconnected -= HandleDisconnected;
    }

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _serverUrlField = root.Q<TextField>("server-url");
        _roomIdField = root.Q<TextField>("room-id");
        _connectBtn = root.Q<Button>("connect-btn");
        _joinBtn = root.Q<Button>("join-btn");
        _connectionLabel = root.Q<Label>("connection-label");

        _serverUrlField.value = _client.ServerUrl;
        ApplyTextFieldColors(_serverUrlField);
        ApplyTextFieldColors(_roomIdField);

        _connectBtn.clicked += OnConnectClicked;
        _joinBtn.clicked += OnJoinClicked;

        UpdateConnectionLabel(_client.State);
        UpdateButtons(_client.State);
    }

    private void OnConnectClicked()
    {
        if (_client.State == NetworkConnectionState.Disconnected)
        {
            _joinAfterConnect = false;
            _pendingRoomId = null;
            _client.SetServerUrl(_serverUrlField.value.Trim());
            _client.Connect();
            return;
        }

        _joinAfterConnect = false;
        _pendingRoomId = null;
        _client.Disconnect();
    }

    private void OnJoinClicked()
    {
        if (_client.State == NetworkConnectionState.Connecting) return;

        var roomId = NormalizeRoomId(_roomIdField.value);

        if (_client.State == NetworkConnectionState.Disconnected)
        {
            _joinAfterConnect = true;
            _pendingRoomId = roomId;
            _client.SetServerUrl(_serverUrlField.value.Trim());
            _client.Connect();
            _connectionLabel.text = "Connecting, then joining room...";
            return;
        }

        _client.JoinRoom(roomId);
    }

    private void HandleConnected()
    {
        if (!_joinAfterConnect) return;

        _joinAfterConnect = false;
        _client.JoinRoom(_pendingRoomId);
        _pendingRoomId = null;
    }

    private void HandleStateChanged(NetworkConnectionState state)
    {
        UpdateConnectionLabel(state);
        UpdateButtons(state);
    }

    private void HandleRoomJoined(RoomJoinedMessage msg)
    {
        _roomIdField.SetValueWithoutNotify(msg.payload.roomId);
        _connectionLabel.text =
            $"Room {msg.payload.roomId} — you are {msg.payload.yourPlayer} ({msg.payload.playerCount}/2)";
    }

    private void HandleServerError(ErrorMessage msg)
    {
        _connectionLabel.text = $"Error: {msg.payload.message}";
    }

    private void HandleDisconnected(string reason)
    {
        _joinAfterConnect = false;
        _pendingRoomId = null;
        _connectionLabel.text = string.IsNullOrEmpty(reason) ? "Disconnected" : $"Disconnected — {reason}";
        UpdateButtons(NetworkConnectionState.Disconnected);
    }

    private void UpdateConnectionLabel(NetworkConnectionState state)
    {
        if (_joinAfterConnect && state == NetworkConnectionState.Connecting) return;

        _connectionLabel.text = state switch
        {
            NetworkConnectionState.Connecting => "Connecting...",
            NetworkConnectionState.Connected => "Connected — join or create a room",
            _ => "Disconnected — Connect, or enter a room code and Join"
        };
    }

    private void UpdateButtons(NetworkConnectionState state)
    {
        var connected = state == NetworkConnectionState.Connected;
        var busy = state == NetworkConnectionState.Connecting;

        _connectBtn.text = connected ? "Disconnect" : "Connect";
        _connectBtn.SetEnabled(!busy);
        _joinBtn.SetEnabled(!busy);
        _serverUrlField.SetEnabled(!connected && !busy);
        _roomIdField.SetEnabled(!busy);
    }

    private static string NormalizeRoomId(string raw)
    {
        var trimmed = raw.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed.ToUpperInvariant();
    }

    private static void ApplyTextFieldColors(TextField field)
    {
        field.style.color = new Color(0.92f, 0.92f, 0.92f);

        var textInput = field.Q(className: "unity-text-input");
        if (textInput != null)
        {
            textInput.style.backgroundColor = new Color(0.06f, 0.2f, 0.37f);
            textInput.style.color = new Color(0.92f, 0.92f, 0.92f);
        }

        var textElement = field.Q(className: "unity-text-element");
        if (textElement != null)
            textElement.style.color = new Color(0.92f, 0.92f, 0.92f);
    }
}
