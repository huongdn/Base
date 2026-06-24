public class GameLogic
{
    private static readonly int[][] WinConditions = new[]
    {
        new[] { 0, 1, 2 }, // rows
        new[] { 3, 4, 5 },
        new[] { 6, 7, 8 },
        new[] { 0, 3, 6 }, // cols
        new[] { 1, 4, 7 },
        new[] { 2, 5, 8 },
        new[] { 0, 4, 8 }, // diagonals
        new[] { 2, 4, 6 },
    };

    public bool TryMakeMove(GameState state, int cellIndex)
    {
        if (state.IsGameOver) return false;
        if (state.Board[cellIndex] != Player.None) return false;

        state.SetCell(cellIndex, state.CurrentPlayer);

        if (CheckWinner(state.Board, state.CurrentPlayer, out int[] winningCells))
            state.SetWinner(state.CurrentPlayer, winningCells);
        else if (CheckDraw(state.Board))
            state.SetDraw();
        else
            state.SwitchPlayer();

        return true;
    }

    private bool CheckWinner(Player[] board, Player player)
    {
        foreach (var condition in WinConditions)
            if (board[condition[0]] == player &&
                board[condition[1]] == player &&
                board[condition[2]] == player)
                return true;
        return false;
    }

    private bool CheckDraw(Player[] board)
    {
        foreach (var cell in board)
            if (cell == Player.None) return false;
        return true;
    }

    private bool CheckWinner(Player[] board, Player player, out int[] winningCells)
    {
        foreach (var condition in WinConditions)
        {
            if (board[condition[0]] == player &&
                board[condition[1]] == player &&
                board[condition[2]] == player)
            {
                winningCells = condition;
                return true;
            }
        }
        winningCells = new int[0];
        return false;
    }
}