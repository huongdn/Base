# TicTacToe Multiplayer — Project Plan

## Stack
- **Client**: Unity 6 (UI Toolkit)
- **Server**: Node.js + TypeScript + `ws` (WebSocket library)
- **Deploy**: Render (free tier)
- **CI/CD**: GitHub Actions
- **Repo**: Monorepo (client + server cùng 1 repo)

## Folder Structure

```
/
├── client/                  # Unity 6 project
│   └── Assets/_Project/
│       ├── Scripts/
│       │   ├── Core/        # GameState, GameLogic — pure C#, không Unity
│       │   ├── UI/          # MonoBehaviour, chỉ lo render
│       │   └── Network/     # WebSocket client (Phase 3)
│       ├── Scenes/
│       ├── Prefabs/
│       └── Art/
│           └── UI/          # UXML + USS files
├── server/                  # Node.js TypeScript
│   ├── src/
│   │   ├── games/           # TicTacToe, Caro, BattleShip logic
│   │   └── core/            # WebSocket handler, room manager
│   └── Dockerfile
├── .github/
│   └── workflows/           # CI/CD pipeline
├── PLAN.md
└── PROGRESS.md
```

## Architecture Principles
- **Server = source of truth** — client không tự quyết game state
- **Game Logic độc lập với Unity** — pure C#, dễ port lên server
- **Client chỉ lo render + gửi/nhận event**
- **Mỗi phase commit rõ ràng, PR có description**

---

## Phase 1 — Local TicTacToe (Unity only)
> Mục tiêu: quen Unity, code game logic clean, tách biệt rõ ràng

- [ ] 1.1 Setup Unity project + folder structure
- [ ] 1.2 Board 3x3 với UI Toolkit (UXML + USS)
- [ ] 1.3 Game Logic: `GameState`, `GameLogic` (pure C#)
- [ ] 1.4 `GameUI` MonoBehaviour — kết nối Logic và UI
- [ ] 1.5 Polish: highlight ô thắng, restart button, UX cơ bản

---

## Phase 2 — Setup Server + CI/CD
> Mục tiêu: quen Node.js, GitHub Actions, Render deploy

- [ ] 2.1 Init Node.js + TypeScript project
- [ ] 2.2 WebSocket server cơ bản (`ws` library)
- [ ] 2.3 Setup GitHub repo + branching strategy
- [ ] 2.4 Viết GitHub Actions workflow (lint → build → deploy)
- [ ] 2.5 Connect Render với GitHub repo
- [ ] 2.6 Deploy lần đầu, verify CI/CD chạy đúng

---

## Phase 3 — Multiplayer TicTacToe
> Mục tiêu: connect Unity client lên server thật

- [ ] 3.1 Thiết kế message protocol (JSON schema)
  - `join_room`, `make_move`, `game_state`, `game_over`
- [ ] 3.2 Server: room manager, game session
- [ ] 3.3 Unity: WebSocket client, connect/disconnect handling
- [ ] 3.4 Sync game state — server là source of truth
- [ ] 3.5 Handle edge cases: player disconnect, reconnect

---

## Phase 4 — Polish + Scale
> Mục tiêu: chuẩn bị mở rộng lên Caro / BattleShip

- [ ] 4.1 Lobby UI: tạo phòng, join phòng bằng code
- [ ] 4.2 Basic matchmaking (2 người vào → tự ghép)
- [ ] 4.3 Refactor server: tách game logic riêng (dễ thêm game mới)
- [ ] 4.4 Viết README + document protocol

---

## Timeline
| Phase | Thời gian ước tính | Milestone |
|---|---|---|
| Phase 1 | 1–2 tuần | Chơi được local 2 players |
| Phase 2 | 3–5 ngày | Push code → tự deploy lên Render |
| Phase 3 | 1–2 tuần | 2 người chơi online được |
| Phase 4 | 1 tuần | Có lobby, code sạch, sẵn sàng scale |

---

## Roadmap tương lai
- **Caro (Gomoku)**: board size thay đổi, win condition algorithm, có thể thêm Redis
- **BattleShip**: turn-based + hidden state (fog of war), học thêm security/cheat prevention
