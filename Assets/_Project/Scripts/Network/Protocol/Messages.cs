using System;

/// <summary>JSON DTOs for WebSocket messages. Field names match Documents/PROTOCOL.md.</summary>

// --- Client → Server ---

[Serializable]
public class JoinRoomMessage
{
    public string type = MessageTypes.JoinRoom;
    public JoinRoomPayload payload = new();
}

[Serializable]
public class JoinRoomPayload
{
    public string roomId;
}

[Serializable]
public class MakeMoveMessage
{
    public string type = MessageTypes.MakeMove;
    public MakeMovePayload payload = new();
}

[Serializable]
public class MakeMovePayload
{
    public int cellIndex;
}

[Serializable]
public class RejoinRoomMessage
{
    public string type = MessageTypes.RejoinRoom;
    public RejoinRoomPayload payload = new();
}

[Serializable]
public class RejoinRoomPayload
{
    public string roomId;
    public string sessionToken;
}

// --- Server → Client ---

[Serializable]
public class RoomJoinedMessage
{
    public string type;
    public RoomJoinedPayload payload;
}

[Serializable]
public class RoomJoinedPayload
{
    public string roomId;
    public string yourPlayer;
    public string sessionToken;
    public int playerCount;
}

[Serializable]
public class GameStateMessage
{
    public string type;
    public GameStatePayload payload;
}

[Serializable]
public class GameStatePayload
{
    public string roomId;
    public string status;
    public string[] board;
    public string currentPlayer;
    public string yourPlayer;
    public string winner;
    public int[] winningCells;
    public bool isDraw;
}

[Serializable]
public class GameOverMessage
{
    public string type;
    public GameOverPayload payload;
}

[Serializable]
public class GameOverPayload
{
    public string roomId;
    public string winner;
    public int[] winningCells;
    public bool isDraw;
}

[Serializable]
public class PlayerLeftMessage
{
    public string type;
    public PlayerLeftPayload payload;
}

[Serializable]
public class PlayerLeftPayload
{
    public string roomId;
    public string leftPlayer;
}

[Serializable]
public class ErrorMessage
{
    public string type;
    public ErrorPayload payload;
}

[Serializable]
public class ErrorPayload
{
    public string code;
    public string message;
}
