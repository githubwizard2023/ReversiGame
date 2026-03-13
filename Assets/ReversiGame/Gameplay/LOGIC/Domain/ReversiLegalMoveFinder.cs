using System.Collections.Generic;

namespace Game
{
    // This class finds every legal move for a given disc color on the current board.
    // It delegates flip detection to the disc flipper so validation rules stay in one place.
    // This keeps legal move logic separate from board state and avoids duplicating the direction scan.
    public class ReversiLegalMoveFinder
    {
        private readonly ReversiDiscFlipper _disc_flipper;

        public ReversiLegalMoveFinder(ReversiDiscFlipper disc_flipper)
        {
            _disc_flipper = disc_flipper;
        }

        // Returns all positions where the given disc color can legally place a disc.
        // A position is legal if the cell is empty and the placement would flip at least one opponent disc.
        public List<BoardPosition> FindLegalMoves(ReversiBoard board, CellState disc_color)
        {
            List<BoardPosition> legal_moves = new List<BoardPosition>();

            for (int row = 0; row < ReversiBoard.BOARD_SIZE; row++)
            {
                for (int column = 0; column < ReversiBoard.BOARD_SIZE; column++)
                {
                    if (board.GetCellState(row, column) != CellState.Empty)
                    {
                        continue;
                    }

                    List<BoardPosition> flipped_positions =
                        _disc_flipper.GetFlippedPositions(board, row, column, disc_color);

                    if (flipped_positions.Count > 0)
                    {
                        legal_moves.Add(new BoardPosition(row, column));
                    }
                }
            }

            return legal_moves;
        }

        // Checks whether the given disc color has at least one legal move available.
        // This avoids generating the full move list when only existence matters.
        public bool HasAnyLegalMove(ReversiBoard board, CellState disc_color)
        {
            for (int row = 0; row < ReversiBoard.BOARD_SIZE; row++)
            {
                for (int column = 0; column < ReversiBoard.BOARD_SIZE; column++)
                {
                    if (board.GetCellState(row, column) != CellState.Empty)
                    {
                        continue;
                    }

                    List<BoardPosition> flipped_positions =
                        _disc_flipper.GetFlippedPositions(board, row, column, disc_color);

                    if (flipped_positions.Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
