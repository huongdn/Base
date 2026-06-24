using UnityEngine;
using UnityEngine.UIElements;

public class GameUI : MonoBehaviour
{
    private GameState _state;
    private GameLogic _logic;

    private Label _statusLabel;
    private Button[] _cells = new Button[9];
    private Button _restartBtn;

    void Start()
    {
        _state = new GameState();
        _logic = new GameLogic();

        var root = GetComponent<UIDocument>().rootVisualElement;

        _statusLabel = root.Q<Label>("status-label");
        _restartBtn = root.Q<Button>("restart-btn");

        for (int i = 0; i < 9; i++)
        {
            int index = i; // capture for lambda
            _cells[i] = root.Q<Button>($"cell-{i}");
            _cells[i].clicked += () => OnCellClicked(index);
        }

        _restartBtn.clicked += OnRestart;

        UpdateUI();
    }

    private void OnCellClicked(int index)
    {
        if (!_logic.TryMakeMove(_state, index)) return;
        UpdateUI();
    }

    private void OnRestart()
    {
        _state.Reset();
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update cells
        for (int i = 0; i < 9; i++)
        {
            _cells[i].text = _state.Board[i] switch
            {
                Player.X => "X",
                Player.O => "O",
                _ => ""
            };
            _cells[i].SetEnabled(!_state.IsGameOver && _state.Board[i] == Player.None);
        }

        // Update status label
        if (_state.Winner != Player.None)
            _statusLabel.text = $"Player {_state.Winner} Wins! 🎉";
        else if (_state.IsDraw)
            _statusLabel.text = "It's a Draw!";
        else
            _statusLabel.text = $"Player {_state.CurrentPlayer}'s Turn";
    }
}