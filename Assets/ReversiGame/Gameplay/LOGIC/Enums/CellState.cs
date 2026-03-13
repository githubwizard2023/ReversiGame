namespace Game
{
    // This enum represents the three possible states of a single cell on the Reversi board.
    // It maps directly to the game's domain model and avoids mixing presentation concerns.
    // This keeps the board state clean and queryable without referencing colors or UI markers.
    public enum CellState
    {
        Empty,
        Black,
        White
    }
}
