using System.Collections.Generic;

namespace Game
{
    // This class validates and executes a complete Reversi move: place a disc and flip captured opponents.
    // It coordinates the flipper and board so callers do not need to handle partial placement logic.
    // This keeps move execution atomic and avoids scattering place-then-flip steps across controllers.
    public class ReversiMoveExecutor
    {
        private readonly ReversiDiscFlipper _disc_flipper;

        public ReversiMoveExecutor(ReversiDiscFlipper disc_flipper)
        {
            _disc_flipper = disc_flipper;
        }

        // Attempts to place a disc at the given position and flip all captured opponent discs.
        // Returns true if the move was legal and executed, false if the cell is occupied or no flips result.
        public bool TryExecuteMove(ReversiBoard board, int row, int column, CellState disc_color)
        {
            if (board.GetCellState(row, column) != CellState.Empty)
            {
                return false;
            }

            List<BoardPosition> flipped_positions =
                _disc_flipper.GetFlippedPositions(board, row, column, disc_color);

            if (flipped_positions.Count == 0)
            {
                return false;
            }

            board.SetCellState(row, column, disc_color);

            foreach (BoardPosition flipped_position in flipped_positions)
            {
                board.SetCellState(flipped_position.row, flipped_position.column, disc_color);
            }

            return true;
        }
    }
}
