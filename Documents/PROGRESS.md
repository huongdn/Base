# TicTacToe Multiplayer — Progress Tracker

## Phase 1 — Local TicTacToe (Unity only)

- [x] **1.1** Setup Unity 6 project + folder structure

  - Template: 2D URP

  - Structure verified: `Assets/_Project/Scripts/{Core,UI,Network}`, `Art/UI/`, `Scenes/`, `Prefabs/`

- [x] **1.2** 3x3 Board layout with UI Toolkit

  - Created `GameBoard.uxml` — 3x3 grid layout, status label, interactive restart button

  - Created `GameBoard.uss` — Dark theme styling, consistent 100x100px cell constraints

- [x] **1.3** Game Logic (pure C#, no UnityEngine dependency)

  - Implemented `Assets/_Project/Scripts/Core/GameState.cs`

    - Encapsulated fields: `Player[] Board`, `CurrentPlayer`, `Winner`, `IsDraw`, `IsGameOver`

    - Lifecycle methods: `Reset()`, `SetWinner()`, `SetDraw()`, `SetCell()`, `SwitchPlayer()`

  - Implemented `Assets/_Project/Scripts/Core/GameLogic.cs`

    - Evaluation methods: `TryMakeMove()`, `CheckWinner()`, `CheckDraw()`

    - Win conditions: 8 layout vectors mapped (3 rows, 3 columns, 2 diagonals)

- [x] **1.4** `GameUI` MonoBehaviour

  - Connected network/core logic layers to UI Toolkit elements via `Assets/_Project/Scripts/UI/GameUI.cs`

  - Resolved compiler error `CS0246: GameLogic could not be found` (assembly reference and layout indexing fixed)

- [x] **1.5** Polish: Basic UX loop implementation completed

---

## Phase 2 — Server + CI/CD

- [x] **2.1** Initialize Node.js + TypeScript environment

- [x] **2.2** Deploy minimal native WebSocket instance using `ws`

- [x] **2.3** Establish Git branching strategy and repository policies

- [x] **2.4** Author robust GitHub Actions workflow file `server-deploy.yml`) containing strict type-checking step

- [x] **2.5** Configure production endpoint webhook securely with GitHub Repository Secrets `RENDER_DEPLOY_HOOK`)

- [x] **2.6** Run initial deployment pipeline check — production service live verification complete

---

## Phase 3 — Multiplayer

> In Progress — see [PHASE3-IMPLEMENTATION.md](PHASE3-IMPLEMENTATION.md) for full details

- [x] **3.1** Design communication protocol (JSON schema)
  - [PROTOCOL.md](PROTOCOL.md), `server/src/protocol/`, Unity `Network/Protocol/` DTOs

- [x] **3.2** Server: room manager + game session lifecycle
  - `server/src/core/`, `server/src/games/tictactoe/`, protocol-compliant WebSocket handlers

- [ ] **3.3** Unity: WebSocket network client, connection handling

- [ ] **3.4** State synchronization — server as source of truth in `GameUI`

- [ ] **3.5** Edge cases: disconnect, reconnect (`rejoin_room`)

---

## Phase 4 — Polish + Scale

> Pending Phase 3 completion

---

## Notes & Decisions

- **Hosting**: Render selected for backend deployment (Free tier constraints accepted, including the 15-minute spin-down behavior).

- **UI Architecture**: UI Toolkit preferred over legacy uGUI for future-proofing and performance native to Unity 6+.

- **Assembly Integration**: Omitted namespaces inside Core scripts temporarily to simplify assembly definition mapping inside Unity's assembly pipeline.

- **Iteration Speed Optimization**: Configured Enter Play Mode Settings (disabled Domain Reload + Scene Reload) to bypass loading penalties.

- **Workflow Control**: Disabled Unity Auto-Refresh; manually polling workspace changes with explicit compilation triggers (Ctrl+R).

## Blockers

- None. Phase 1 bugs resolved; Phase 2 successfully verified green. Ready for network integration layers.