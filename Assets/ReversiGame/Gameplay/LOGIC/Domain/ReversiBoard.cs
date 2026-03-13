namespace Game
{
    // This class owns the 8x8 grid state for a single Reversi match.
    // It is a pure data holder that does not contain move validation or flip logic.
    // This keeps the board readable and lets other domain services operate on it without coupling.
    public class ReversiBoard
    {
        public const int BOARD_SIZE = 8;

        private const int CENTER_LOW = 3;
        private const int CENTER_HIGH = 4;

        private readonly CellState[,] _cells;

        public ReversiBoard()
        {
            _cells = new CellState[BOARD_SIZE, BOARD_SIZE];
        }

        // Returns the current state of the cell at the given position.
        public CellState GetCellState(int row, int column)
        {
            return _cells[row, column];
        }

        // Overwrites the cell at the given position with the supplied state.
        // This is a raw setter; validation should happen before calling this method.
        public void SetCellState(int row, int column, CellState cell_state)
        {
            _cells[row, column] = cell_state;
        }

        // Places the classic four-disc starting pattern in the center of the board.
        // In standard Reversi, white occupies d4 and e5; black occupies d5 and e4.
        public void SetupInitialPosition()
        {
            ClearAllCells();

            _cells[CENTER_LOW, CENTER_LOW] = CellState.White;
            _cells[CENTER_LOW, CENTER_HIGH] = CellState.Black;
            _cells[CENTER_HIGH, CENTER_LOW] = CellState.Black;
            _cells[CENTER_HIGH, CENTER_HIGH] = CellState.White;
        }

        // Creates a deep copy of this board so AI evaluation can explore without mutating the live game state.
        public ReversiBoard Clone()
        {
            ReversiBoard cloned_board = new ReversiBoard();

            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int column = 0; column < BOARD_SIZE; column++)
                {
                    cloned_board._cells[row, column] = _cells[row, column];
                }
            }

            return cloned_board;
        }

        // Resets every cell to empty so the board can be reused or initialized cleanly.
        private void ClearAllCells()
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int column = 0; column < BOARD_SIZE; column++)
                {
                    _cells[row, column] = CellState.Empty;
                }
            }
        }
    }
}
