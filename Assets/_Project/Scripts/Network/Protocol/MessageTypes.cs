/// <summary>Wire protocol type strings — must match Documents/PROTOCOL.md.</summary>
public static class MessageTypes
{
    // Client → Server
    public const string JoinRoom = "join_room";
    public const string MakeMove = "make_move";
    public const string RejoinRoom = "rejoin_room";

    // Server → Client
    public const string RoomJoined = "room_joined";
    public const string GameState = "game_state";
    public const string GameOver = "game_over";
    public const string PlayerLeft = "player_left";
    public const string Error = "error";
}

public static class RoomStatus
{
    public const string Waiting = "waiting";
    public const string Playing = "playing";
    public const string Finished = "finished";
}

public static class ErrorCodes
{
    public const string InvalidMessage = "INVALID_MESSAGE";
    public const string InvalidPayload = "INVALID_PAYLOAD";
    public const string RoomNotFound = "ROOM_NOT_FOUND";
    public const string RoomFull = "ROOM_FULL";
    public const string NotInRoom = "NOT_IN_ROOM";
    public const string NotYourTurn = "NOT_YOUR_TURN";
    public const string InvalidMove = "INVALID_MOVE";
    public const string GameOver = "GAME_OVER";
    public const string SessionInvalid = "SESSION_INVALID";
}
