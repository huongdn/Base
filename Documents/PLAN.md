# TicTacToe Multiplayer — Project Plan

## Stack

- **Client**: Unity 6 (UI Toolkit)

- **Server**: Node.js + TypeScript + `ws` (WebSocket library)

- **Deploy**: Render (free tier)

- **CI/CD**: GitHub Actions

- **Repo**: Monorepo (client + server in the same repository)

## Folder Structure

/

├── client/                  # Unity 6 project

│   └── Assets/_Project/

│       ├── Scripts/

│       │   ├── Core/        # GameState, GameLogic — pure C#, no Unity dependencies

│       │   ├── UI/          # MonoBehaviour, rendering and input handling only

│       │   └── Network/     # WebSocket client (Phase 3)

│       ├── Scenes/

│       ├── Prefabs/

│       └── Art/

│           └── UI/          # UXML + USS files

├── server/                  # Node.js TypeScript

│   ├── src/

│   │   ├── games/           # TicTacToe game logic (extensible to Caro, BattleShip)

│   │   └── core/            # WebSocket handlers, room management

│   └── Dockerfile

├── .github/

│   └── workflows/           # CI/CD pipelines

├── [PLAN.md](http://PLAN.md)

└── [PROGRESS.md](http://PROGRESS.md)

## Architecture Principles

- **Server as the Source of Truth** — Client does not determine or mutate the game state locally.

- **Engine-Agnostic Game Logic** — Pure C# for core logic, allowing easy porting or reuse.

- **Thin Client** — Client focuses solely on rendering, UI animation, and event transmission.

- **Atomic Commits & Descriptive PRs** — Every phase requires clean commits and structured PR descriptions.

---

## Phase 1 — Local TicTacToe (Unity only)

> Objective: Familiarize with Unity 6 features, implement clean logic separation.

- [x] 1.1 Setup Unity project + folder structure

- [x] 1.2 3x3 Board layout with UI Toolkit (UXML + USS)

- [x] 1.3 Game Logic: `GameState`, `GameLogic` (pure C#)

- [x] 1.4 `GameUI` MonoBehaviour — bind Logic to UI

- [x] 1.5 Polish: Winning cells highlight, restart flow, basic UX refinement

---

## Phase 2 — Server Setup + CI/CD

> Objective: Initialize Node.js backend, configure automated pipelines, and deploy to production.

- [x] 2.1 Initialize Node.js + TypeScript project

- [x] 2.2 Basic WebSocket server using `ws` library

- [x] 2.3 Setup GitHub repository + branching strategy

- [x] 2.4 Configure GitHub Actions workflow (lint → typecheck → build → deploy)

- [x] 2.5 Hook Render deployment with GitHub repository secrets

- [x] 2.6 Execute initial deployment and verify production log correctness

---

## Phase 3 — Multiplayer TicTacToe

> Objective: Connect Unity client to the production server via WebSockets.

- [ ] 3.1 Design communication protocol (JSON schema)

  - Messages: `join_room`, `make_move`, `game_state`, `game_over`

- [ ] 3.2 Server implementation: room manager, game session lifecycle

- [ ] 3.3 Unity implementation: WebSocket network client, connection handling

- [ ] 3.4 State synchronization — enforce server as source of truth

- [ ] 3.5 Edge case handling: connection drops, graceful degradation, and reconnects

---

## Phase 4 — Polish + Scale

> Objective: Abstract interfaces to prepare for project scaling (Caro / BattleShip).

- [ ] 4.1 Lobby UI: Room creation and joining via room codes

- [ ] 4.2 Basic matchmaking queue (2 active players automatically match)

- [ ] 4.3 Server-side refactoring: Decouple room core from game-specific components

- [ ] 4.4 Documentation: Technical README + explicit message protocol reference

---

## Timeline

| Phase | Estimated Duration | Milestone |

|---|---|---|

| Phase 1 | 1–2 Weeks | Fully playable local 2-player mode |

| Phase 2 | 3–5 Days | Automated CD trigger to Render on main push |

| Phase 3 | 1–2 Weeks | Stable online multiplayer gameplay loop |

| Phase 4 | 1 Week | Scalable codebase with matchmaking capability |

---

## Future Roadmap

- **Caro (Gomoku)**: Dynamic board sizes, continuous win-condition algorithms, potential Redis integration for sessions.

- **BattleShip**: Asymmetric turn-based state (fog of war), security analysis, and server-side cheat prevention.