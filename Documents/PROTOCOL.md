# TicTacToe Multiplayer — WebSocket Protocol (Phase 3.1)

All messages are JSON text frames over WebSocket. Every message uses an envelope:

```json
{
  "type": "<message_type>",
  "payload": { ... }
}
```

- `type` — string discriminator (snake_case)
- `payload` — object; shape depends on `type`

Board cells use index `0–8` (row-major, top-left to bottom-right):

```
0 | 1 | 2
---------
3 | 4 | 5
---------
6 | 7 | 8
```

Player symbols: `"X"`, `"O"`, `"None"` (empty cell).

---

## Client → Server

### `join_room`

Join an existing room or create one when `roomId` is omitted.

```json
{
  "type": "join_room",
  "payload": {
    "roomId": "ABC123"
  }
}
```

| Field    | Type   | Required | Description                          |
|----------|--------|----------|--------------------------------------|
| `roomId` | string | no       | 6-char room code. Omit to create new |

### `make_move`

Place the current player's mark on a cell. Server validates turn and cell.

```json
{
  "type": "make_move",
  "payload": {
    "cellIndex": 4
  }
}
```

| Field       | Type   | Required | Description        |
|-------------|--------|----------|--------------------|
| `cellIndex` | number | yes      | Integer `0–8`      |

### `rejoin_room` (Phase 3.5)

Reconnect with a session token issued when the player first joined.

```json
{
  "type": "rejoin_room",
  "payload": {
    "roomId": "ABC123",
    "sessionToken": "uuid-string"
  }
}
```

---

## Server → Client

### `room_joined`

Sent after a successful `join_room` / `rejoin_room`. Tells the client its assigned symbol and room.

```json
{
  "type": "room_joined",
  "payload": {
    "roomId": "ABC123",
    "yourPlayer": "X",
    "sessionToken": "uuid-string",
    "playerCount": 1
  }
}
```

| Field          | Type   | Description                              |
|----------------|--------|------------------------------------------|
| `roomId`       | string | Room code                                |
| `yourPlayer`   | string | `"X"` or `"O"`                           |
| `sessionToken` | string | Save for `rejoin_room`                   |
| `playerCount`  | number | `1` = waiting, `2` = ready to play     |

### `game_state`

Authoritative game snapshot. Server broadcasts on join, after each move, and when status changes.

```json
{
  "type": "game_state",
  "payload": {
    "roomId": "ABC123",
    "status": "playing",
    "board": ["X", "None", "O", "None", "X", "None", "None", "None", "None"],
    "currentPlayer": "O",
    "yourPlayer": "X",
    "winner": "None",
    "winningCells": [],
    "isDraw": false
  }
}
```

| Field           | Type     | Description                                      |
|-----------------|----------|--------------------------------------------------|
| `roomId`        | string   | Room code                                        |
| `status`        | string   | `"waiting"` \| `"playing"` \| `"finished"`       |
| `board`         | string[] | Length `9`; each cell `"X"` \| `"O"` \| `"None"` |
| `currentPlayer` | string   | Whose turn: `"X"` or `"O"`                       |
| `yourPlayer`    | string   | This client's symbol                             |
| `winner`        | string   | `"X"` \| `"O"` \| `"None"`                       |
| `winningCells`  | number[] | Winning line indices; empty if no winner         |
| `isDraw`        | boolean  | `true` when board full with no winner            |

**Status flow**

1. `waiting` — one player in room; board empty
2. `playing` — two players; moves allowed
3. `finished` — win or draw; no more moves accepted

### `game_over`

Sent once when the game ends. Payload is a subset of the final `game_state` for clear client handling.

```json
{
  "type": "game_over",
  "payload": {
    "roomId": "ABC123",
    "winner": "X",
    "winningCells": [0, 1, 2],
    "isDraw": false
  }
}
```

Clients should still apply the accompanying `game_state` (or the last one) as source of truth for the board.

### `player_left` (Phase 3.5)

Opponent disconnected before the game finished.

```json
{
  "type": "player_left",
  "payload": {
    "roomId": "ABC123",
    "leftPlayer": "O"
  }
}
```

### `error`

Request rejected or malformed message.

```json
{
  "type": "error",
  "payload": {
    "code": "NOT_YOUR_TURN",
    "message": "It is not your turn."
  }
}
```

| Code               | When                                      |
|--------------------|-------------------------------------------|
| `INVALID_MESSAGE`  | JSON parse failure or unknown `type`      |
| `INVALID_PAYLOAD`  | Missing or invalid fields in `payload`    |
| `ROOM_NOT_FOUND`   | `roomId` does not exist                   |
| `ROOM_FULL`        | Room already has 2 players                |
| `NOT_IN_ROOM`      | `make_move` before `join_room`            |
| `NOT_YOUR_TURN`    | Move attempted on opponent's turn         |
| `INVALID_MOVE`     | Cell occupied or `cellIndex` out of range |
| `GAME_OVER`        | Move attempted after game finished        |
| `SESSION_INVALID`  | `rejoin_room` token not recognized        |

---

## Typical flow

```
Client A                    Server                    Client B
   |                          |                          |
   |-- join_room (no id) ---->|                          |
   |<- room_joined (X) -------|                          |
   |<- game_state (waiting) --|                          |
   |                          |<---- join_room (ABC) ----|
   |                          |----- room_joined (O) ---->|
   |<- game_state (playing) --|---- game_state (playing)->|
   |                          |                          |
   |-- make_move (cell 4) --->|                          |
   |<- game_state ------------|---- game_state --------->|
   |                          |                          |
   |                          |<---- make_move (cell 0) -|
   |<- game_state ------------|---- game_state --------->|
   |                          |                          |
   |        ... until win or draw ...                    |
   |<- game_over -------------|---- game_over ----------->|
   |<- game_state (finished) -|---- game_state (finished)->|
```

---

## Implementation files

| Location | Purpose |
|----------|---------|
| `Documents/PROTOCOL.md` | This document |
| `server/src/protocol/schema.json` | JSON Schema (draft-07) |
| `server/src/protocol/types.ts` | TypeScript types |
| `server/src/protocol/parse.ts` | Parse + validate incoming client messages |
| `Assets/_Project/Scripts/Network/Protocol/` | Unity DTOs (JsonUtility-compatible) |
