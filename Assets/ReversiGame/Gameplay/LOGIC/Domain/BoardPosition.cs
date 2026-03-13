namespace Game
{
    // This struct represents a discrete position on the 8x8 Reversi board.
    // It replaces raw int pairs so position data is self-documenting throughout domain code.
    // This keeps move lists and flip results typed instead of relying on unnamed tuple fields.
    public readonly struct BoardPosition
    {
        public BoardPosition(int row, int column)
        {
            this.row = row;
            this.column = column;
        }

        public int row { get; }

        public int column { get; }
    }
}
