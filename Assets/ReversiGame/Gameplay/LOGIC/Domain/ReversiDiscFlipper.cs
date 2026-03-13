using System.Collections.Generic;

namespace Game
{
    // This class resolves which opponent discs get flipped when a disc is placed on a given cell.
    // It encapsulates the eight-direction scan that is central to Reversi's capture mechanic.
    // This keeps flip resolution reusable for both move validation and move execution.
    public class ReversiDiscFlipper
    {
        private const int DIRECTION_COUNT = 8;

        // Row offsets for the eight compass directions: N, NE, E, SE, S, SW, W, NW.
        private static readonly int[] ROW_DIRECTIONS = { -1, -1, 0, 1, 1, 1, 0, -1 };

        // Column offsets for the eight compass directions: N, NE, E, SE, S, SW, W, NW.
        private static readonly int[] COLUMN_DIRECTIONS = { 0, 1, 1, 1, 0, -1, -1, -1 };

        // Returns all board positions whose discs would flip if the given disc color is placed at (row, column).
        // An empty list means no captures, which also means the move is not legal.
        public List<BoardPosition> GetFlippedPositions(ReversiBoard board, int row, int column, CellState disc_color)
        {
            List<BoardPosition> all_flipped_positions = new List<BoardPosition>();

            for (int direction_index = 0; direction_index < DIRECTION_COUNT; direction_index++)
            {
                List<BoardPosition> flipped_in_direction = ScanDirection(
                    board, row, column, disc_color,
                    ROW_DIRECTIONS[direction_index],
                    COLUMN_DIRECTIONS[direction_index]);

                all_flipped_positions.AddRange(flipped_in_direction);
            }

            return all_flipped_positions;
        }

        // Walks one direction from the placed position and collects opponent discs that are bracketed.
        // The scan stops when it hits an empty cell, a board edge, or the placing player's own disc.
        private static List<BoardPosition> ScanDirection(
            ReversiBoard board, int start_row, int start_column,
            CellState disc_color, int row_step, int column_step)
        {
            List<BoardPosition> candidates = new List<BoardPosition>();
            CellState opponent_color = GetOpponentColor(disc_color);

            int current_row = start_row + row_step;
            int current_column = start_column + column_step;

            while (IsInsideBoard(current_row, current_column))
            {
                CellState current_cell = board.GetCellState(current_row, current_column);

                if (current_cell == opponent_color)
                {
                    candidates.Add(new BoardPosition(current_row, current_column));
                    current_row += row_step;
                    current_column += column_step;
                    continue;
                }

                if (current_cell == disc_color && candidates.Count > 0)
                {
                    return candidates;
                }

                break;
            }

            return new List<BoardPosition>();
        }

        // Returns the opponent disc color for the given player.
        private static CellState GetOpponentColor(CellState disc_color)
        {
            if (disc_color == CellState.Black)
            {
                return CellState.White;
            }

            return CellState.Black;
        }

        // Checks whether a position falls within the valid 8x8 board boundaries.
        private static bool IsInsideBoard(int row, int column)
        {
            return row >= 0 && row < ReversiBoard.BOARD_SIZE
                && column >= 0 && column < ReversiBoard.BOARD_SIZE;
        }
    }
}
