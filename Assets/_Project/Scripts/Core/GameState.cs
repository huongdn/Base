public enum Player { None, X, O }

public class GameState
{
    public Player[] Board { get; private set; } = new Player[9];
    public Player CurrentPlayer { get; private set; } = Player.X;
    public Player Winner { get; private set; } = Player.None;
    public bool IsDraw { get; private set; } = false;
    public bool IsGameOver => Winner != Player.None || IsDraw;

    public void Reset()
    {
        Board = new Player[9];
        CurrentPlayer = Player.X;
        Winner = Player.None;
        IsDraw = false;
    }

    public void SetWinner(Player player) => Winner = player;
    public void SetDraw() => IsDraw = true;
    public void SetCell(int index, Player player) => Board[index] = player;
    public void SwitchPlayer() => CurrentPlayer = CurrentPlayer == Player.X ? Player.O : Player.X;
}