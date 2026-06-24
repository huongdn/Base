# TicTacToe Multiplayer — Progress Tracker

## Phase 1 — Local TicTacToe (Unity only)

- [x] **1.1** Setup Unity 6 project + folder structure
  - Template: 2D URP
  - Structure: `Assets/_Project/Scripts/{Core,UI,Network}`, `Art/UI/`, `Scenes/`, `Prefabs/`

- [x] **1.2** Board 3x3 với UI Toolkit
  - `GameBoard.uxml` — layout 3x3, status label, restart button
  - `GameBoard.uss` — dark theme, cell 100x100px

- [x] **1.3** Game Logic (pure C#, no UnityEngine)
  - `Assets/_Project/Scripts/Core/GameState.cs`
    - `Player[] Board`, `CurrentPlayer`, `Winner`, `IsDraw`, `IsGameOver`
    - Methods: `Reset()`, `SetWinner()`, `SetDraw()`, `SetCell()`, `SwitchPlayer()`
  - `Assets/_Project/Scripts/Core/GameLogic.cs`
    - `TryMakeMove()`, `CheckWinner()`, `CheckDraw()`
    - Win conditions: 8 tổ hợp (3 hàng, 3 cột, 2 đường chéo)

- [ ] **1.4** `GameUI` MonoBehaviour
  - `Assets/_Project/Scripts/UI/GameUI.cs`
  - ⚠️ Đang bị lỗi: `CS0246 - GameLogic could not be found`
  - **TODO**: Reimport All hoặc kiểm tra lại file encoding

- [ ] **1.5** Polish: highlight ô thắng, UX

---

## Phase 2 — Server + CI/CD
> Chưa bắt đầu

---

## Phase 3 — Multiplayer
> Chưa bắt đầu

---

## Phase 4 — Polish + Scale
> Chưa bắt đầu

---

## Notes & Decisions
- **Render** được chọn làm deployment platform (free tier, chấp nhận sleep 15 phút)
- **UI Toolkit** thay vì uGUI — forward-compatible với Unity 6+
- **Không dùng namespace** trong Core scripts để Unity tự resolve assembly
- Enter Play Mode Settings: bật, tắt Reload Domain + Reload Scene để vào Play Mode nhanh hơn
- Auto Refresh: tắt, dùng Ctrl+R thủ công

## Blockers
- [ ] Lỗi compile `CS0246` ở `GameUI.cs` — chờ Reimport xong để kiểm tra
