using System;
using UnityEngine;

/// <summary>Serialize and deserialize protocol messages for JsonUtility.</summary>
public static class MessageSerializer
{
    [Serializable]
    private class TypeProbe
    {
        public string type;
    }

    public static string SerializeJoinRoom(string roomId = null)
    {
        if (string.IsNullOrEmpty(roomId))
            return "{\"type\":\"join_room\",\"payload\":{}}";

        var message = new JoinRoomMessage
        {
            payload = new JoinRoomPayload { roomId = roomId }
        };
        return JsonUtility.ToJson(message);
    }

    public static string SerializeMakeMove(int cellIndex)
    {
        var message = new MakeMoveMessage
        {
            payload = new MakeMovePayload { cellIndex = cellIndex }
        };
        return JsonUtility.ToJson(message);
    }

    public static string SerializeRejoinRoom(string roomId, string sessionToken)
    {
        var message = new RejoinRoomMessage
        {
            payload = new RejoinRoomPayload
            {
                roomId = roomId,
                sessionToken = sessionToken
            }
        };
        return JsonUtility.ToJson(message);
    }

    public static void Dispatch(
        string json,
        Action<RoomJoinedMessage> onRoomJoined = null,
        Action<GameStateMessage> onGameState = null,
        Action<GameOverMessage> onGameOver = null,
        Action<PlayerLeftMessage> onPlayerLeft = null,
        Action<ErrorMessage> onError = null,
        Action<string> onUnknown = null)
    {
        if (string.IsNullOrEmpty(json)) return;

        var probe = JsonUtility.FromJson<TypeProbe>(json);
        if (string.IsNullOrEmpty(probe.type))
        {
            onUnknown?.Invoke(json);
            return;
        }

        switch (probe.type)
        {
            case MessageTypes.RoomJoined:
                onRoomJoined?.Invoke(JsonUtility.FromJson<RoomJoinedMessage>(json));
                break;
            case MessageTypes.GameState:
                onGameState?.Invoke(JsonUtility.FromJson<GameStateMessage>(json));
                break;
            case MessageTypes.GameOver:
                onGameOver?.Invoke(JsonUtility.FromJson<GameOverMessage>(json));
                break;
            case MessageTypes.PlayerLeft:
                onPlayerLeft?.Invoke(JsonUtility.FromJson<PlayerLeftMessage>(json));
                break;
            case MessageTypes.Error:
                onError?.Invoke(JsonUtility.FromJson<ErrorMessage>(json));
                break;
            default:
                onUnknown?.Invoke(json);
                break;
        }
    }
}
