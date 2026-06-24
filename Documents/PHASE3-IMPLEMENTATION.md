# Phase 3 — Multiplayer Implementation Log

This document records work completed for **Phase 3** of the TicTacToe multiplayer project. It complements [PLAN.md](PLAN.md) (roadmap) and [PROGRESS.md](PROGRESS.md) (high-level tracker).

**Status:** Steps **3.1** and **3.2** are done. Steps **3.3–3.5** (Unity client, state sync, reconnect polish) are pending.

---

## Overview

Phase 3 connects the Unity client to a real WebSocket server. The server owns all game state; clients only send intents (`join_room`, `make_move`) and render what the server broadcasts.

| Step | Description | Status |
|------|-------------|--------|
| 3.1 | Message protocol (JSON schema + shared types) | Done |
| 3.2 | Server room manager + game session | Done |
| 3.3 | Unity WebSocket client | Done |
| 3.4 | Client state sync (server as source of truth) | Pending |
| 3.5 | Disconnect / reconnect edge cases | Pending |

---

## 3.1 — Message Protocol

### Goal

Define a stable JSON wire format so Unity and Node.js can communicate without ambiguity.

### Deliverables

| File | Purpose |
|------|---------|
| [PROTOCOL.md](PROTOCOL.md) | Human-readable protocol reference |
| `server/src/protocol/types.ts` | TypeScript message types |
| `server/src/protocol/parse.ts` | Parse and validate incoming client messages |
| `server/src/protocol/schema.json` | JSON Schema (draft-07) |
| `server/src/protocol/index.ts` | Public exports |
| `Assets/_Project/Scripts/Network/Protocol/MessageTypes.cs` | Unity message type constants |
| `Assets/_Project/Scripts/Network/Protocol/Messages.cs` | Unity DTOs (`JsonUtility`-compatible) |

### Envelope format

Every WebSocket frame is JSON:

```json
{
  "type": "<message_type>",
  "payload": { }
}
```

### Client → Server messages

| Type | Purpose |
|------|---------|
| `join_room` | Join by `roomId`, or omit `roomId` to create a new room |
| `make_move` | `{ "cellIndex": 0–8 }` |
| `rejoin_room` | `{ "roomId", "sessionToken" }` — reserved for 3.5 |

### Server → Client messages

| Type | Purpose |
|------|---------|
| `room_joined` | Assigned symbol (`X` / `O`), room id, session token |
| `game_state` | Authoritative snapshot (`waiting` → `playing` → `finished`) |
| `game_over` | Terminal event when the game ends |
| `player_left` | Opponent disconnected mid-game |
| `error` | Rejected request with error code |

### Conventions

- Board indices **0–8**, row-major (top-left = 0).
- Cell values: `"X"`, `"O"`, `"None"`.
- Room codes: **6 characters** (uppercase alphanumeric, ambiguous chars excluded).

### Error codes

`INVALID_MESSAGE`, `INVALID_PAYLOAD`, `ROOM_NOT_FOUND`, `ROOM_FULL`, `NOT_IN_ROOM`, `NOT_YOUR_TURN`, `INVALID_MOVE`, `GAME_OVER`, `SESSION_INVALID`

---

## 3.2 — Server Room Manager & Game Session

### Goal

Replace the echo WebSocket server with rooms, player slots, TicTacToe logic, and protocol-compliant message handling.

### Deliverables

| File | Purpose |
|------|---------|
| `server/src/games/tictactoe/GameState.ts` | Server-side board and turn state |
| `server/src/games/tictactoe/GameLogic.ts` | Move validation, win/draw detection (mirrors Unity `GameLogic`) |
| `server/src/games/tictactoe/index.ts` | Exports |
| `server/src/core/Room.ts` | Single room: players, sessions, moves, broadcasts |
| `server/src/core/RoomManager.ts` | Room registry, join/rejoin/move routing, cleanup |
| `server/src/core/connection.ts` | Per-socket message and disconnect handlers |
| `server/src/core/messaging.ts` | `sendMessage` / `sendError` helpers |
| `server/src/core/index.ts` | `createWebSocketServer()` factory |
| `server/src/index.ts` | Server entry point |

### Room lifecycle

```
join_room (no id)     → create room, player = X, status = waiting
join_room (room id)   → player = O, status = playing
make_move             → validate → broadcast game_state
game ends             → broadcast game_over + game_state (finished)
both disconnect       → room removed
one disconnect        → player_left sent; slot kept for rejoin_room
```

### Room behavior

- **First joiner** gets `X`; **second joiner** gets `O`.
- Each player receives a **`sessionToken`** (UUID) in `room_joined` for future `rejoin_room`.
- `game_state` is **personalized** per client (`yourPlayer` field differs per socket).
- Moves are rejected when: not in a room, waiting for opponent, wrong turn, cell taken, or game already finished.
- Empty rooms (no connected sockets) are **deleted** from memory.

### Architecture

```
index.ts
  └── createWebSocketServer()
        ├── RoomManager (all rooms)
        └── connection handler
              ├── parseClientMessage()
              └── RoomManager.handleMessage() / handleDisconnect()
                    └── Room
                          ├── TicTacToeState + TicTacToeLogic
                          └── broadcast game_state / game_over
```

---

## How to run and test

### Start the server locally

```bash
cd server
npm run dev
```

Default port: **3000** (override with `PORT` env var).

### Production build

```bash
cd server
npm run build
npm start
```

CI runs `tsc --noEmit` and `npm run build` on pushes to `main` under `server/**` (see `.github/workflows/server-deploy.yml`).

### Manual smoke test (Node.js)

With the server running, two clients can join and play:

```javascript
// Client A — create room
ws.send(JSON.stringify({ type: "join_room", payload: {} }));

// Read roomId from room_joined response, then Client B joins
ws.send(JSON.stringify({ type: "join_room", payload: { roomId: "ABC123" } }));

// Moves alternate X then O
ws.send(JSON.stringify({ type: "make_move", payload: { cellIndex: 4 } }));
```

Verified flow: two clients join → moves alternate → `game_over` with correct winner → `game_state.status === "finished"`.

---

## 3.3 — Unity WebSocket Client

### Goal

Connect the Unity client to the server over WebSocket, send protocol messages, and surface connection lifecycle events. The board still runs locally in `GameUI` until step 3.4.

### Deliverables

| File | Purpose |
|------|---------|
| `Assets/_Project/Scripts/Network/NetworkClient.cs` | `ClientWebSocket` wrapper — connect, disconnect, send, receive |
| `Assets/_Project/Scripts/Network/MessageSerializer.cs` | JSON serialize/deserialize using protocol DTOs |
| `Assets/_Project/Scripts/Network/NetworkUI.cs` | Connection bar UI — server URL, connect, join room |
| `Assets/Art/UI/GameBoard.uxml` | Network bar added above the game board |
| `Assets/Art/UI/GameBoard.uss` | Styles for network bar |
| `Assets/Scenes/GameScene.unity` | `NetworkClient` + `NetworkUI` on `UIDocument` object |

### NetworkClient API

| Member | Description |
|--------|-------------|
| `Connect()` / `Disconnect()` | Open/close WebSocket to `serverUrl` |
| `JoinRoom(roomId?)` | Send `join_room` (omit id to create) |
| `MakeMove(cellIndex)` | Send `make_move` (used in 3.4) |
| `RejoinRoom(roomId, token)` | Send `rejoin_room` (used in 3.5) |
| `OnConnected`, `OnDisconnected` | Connection lifecycle |
| `OnRoomJoined`, `OnGameState`, `OnGameOver`, `OnPlayerLeft`, `OnServerError` | Server message events |
| `RoomId`, `SessionToken`, `YourPlayer` | Session info from `room_joined` |

Receive loop runs on a background thread; events are dispatched on Unity's main thread via `Update`.

### How to test in Unity

1. Start server: `cd server && npm run dev`
2. Open `GameScene` in Unity, press Play
3. Click **Connect** (default `ws://localhost:3000`)
4. Click **Join Room** with empty room field → creates room, shows room code + your symbol
5. Second client (build or ParrelSync): Connect → enter room code → **Join Room**

Board moves are still local-only until **3.4**.

---

## What is not done yet

### 3.4 — State synchronization

- `GameUI` must stop applying moves locally and instead send `make_move` to the server.
- UI should render from incoming `game_state` payloads only.

### 3.5 — Edge cases

- `rejoin_room` is implemented on the server and exposed on `NetworkClient`, but disconnect UX is not built in the UI.
- `NetworkUI` does not yet show opponent-left or rejoin prompts.

---

## Related documents

| Document | Contents |
|----------|----------|
| [PLAN.md](PLAN.md) | Full project roadmap (Phases 1–4) |
| [PROGRESS.md](PROGRESS.md) | Checkbox-style progress tracker |
| [PROTOCOL.md](PROTOCOL.md) | Detailed message schemas and flow diagrams |

---

## Changelog

| Date | Step | Summary |
|------|------|---------|
| 2026-06-24 | 3.1 | Protocol spec, JSON schema, TS types + parser, Unity DTOs |
| 2026-06-24 | 3.2 | TicTacToe game logic, room manager, WebSocket handlers, integration tested |
| 2026-06-24 | 3.3 | Unity NetworkClient, MessageSerializer, NetworkUI, connection bar in GameBoard |
