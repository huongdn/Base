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

    private void Awake()
    {
        _client = GetComponent<NetworkClient>();
    }

    private void OnEnable()
    {
        _client.OnStateChanged += HandleStateChanged;
        _client.OnRoomJoined += HandleRoomJoined;
        _client.OnServerError += HandleServerError;
        _client.OnDisconnected += HandleDisconnected;
    }

    private void OnDisable()
    {
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

        _connectBtn.clicked += OnConnectClicked;
        _joinBtn.clicked += OnJoinClicked;

        UpdateConnectionLabel(_client.State);
        UpdateButtons(_client.State);
    }

    private void OnConnectClicked()
    {
        if (_client.State == NetworkConnectionState.Disconnected)
        {
            _client.SetServerUrl(_serverUrlField.value.Trim());
            _client.Connect();
            return;
        }

        _client.Disconnect();
    }

    private void OnJoinClicked()
    {
        if (_client.State != NetworkConnectionState.Connected) return;

        var roomId = _roomIdField.value.Trim();
        _client.JoinRoom(string.IsNullOrEmpty(roomId) ? null : roomId.ToUpperInvariant());
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
        _connectionLabel.text = string.IsNullOrEmpty(reason) ? "Disconnected" : $"Disconnected — {reason}";
        UpdateButtons(NetworkConnectionState.Disconnected);
    }

    private void UpdateConnectionLabel(NetworkConnectionState state)
    {
        _connectionLabel.text = state switch
        {
            NetworkConnectionState.Connecting => "Connecting...",
            NetworkConnectionState.Connected => "Connected — join or create a room",
            _ => "Disconnected"
        };
    }

    private void UpdateButtons(NetworkConnectionState state)
    {
        var connected = state == NetworkConnectionState.Connected;
        var busy = state == NetworkConnectionState.Connecting;

        _connectBtn.text = connected ? "Disconnect" : "Connect";
        _connectBtn.SetEnabled(!busy);
        _joinBtn.SetEnabled(connected);
        _serverUrlField.SetEnabled(state == NetworkConnectionState.Disconnected);
    }
}
