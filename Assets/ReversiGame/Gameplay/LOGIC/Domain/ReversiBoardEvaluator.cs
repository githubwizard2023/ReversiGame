namespace Game
{
    // This class evaluates the board to determine scores and detect whether the match has ended.
    // It reads the board and legal move availability without modifying any state.
    // This keeps scoring and game-over checks in one focused service instead of scattering them.
    public class ReversiBoardEvaluator
    {
        private readonly ReversiLegalMoveFinder _legal_move_finder;

        public ReversiBoardEvaluator(ReversiLegalMoveFinder legal_move_finder)
        {
            _legal_move_finder = legal_move_finder;
        }

        // Counts the total number of discs on the board for the given cell state.
        public int CountDiscs(ReversiBoard board, CellState disc_color)
        {
            int count = 0;

            for (int row = 0; row < ReversiBoard.BOARD_SIZE; row++)
            {
                for (int column = 0; column < ReversiBoard.BOARD_SIZE; column++)
                {
                    if (board.GetCellState(row, column) == disc_color)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        // Returns true when neither side can make a legal move, meaning the match must end.
        public bool IsGameOver(ReversiBoard board)
        {
            bool black_can_move = _legal_move_finder.HasAnyLegalMove(board, CellState.Black);
            bool white_can_move = _legal_move_finder.HasAnyLegalMove(board, CellState.White);

            return black_can_move == false && white_can_move == false;
        }

        // Determines the match result from the human player's perspective.
        // The player's disc color decides how disc counts map to win, lose, or draw.
        public MatchOutcome DetermineOutcome(ReversiBoard board, CellState player_disc_color)
        {
            CellState ai_disc_color = GetOpponentColor(player_disc_color);

            int player_count = CountDiscs(board, player_disc_color);
            int ai_count = CountDiscs(board, ai_disc_color);

            if (player_count > ai_count)
            {
                return MatchOutcome.HumanWin;
            }

            if (ai_count > player_count)
            {
                return MatchOutcome.AIWin;
            }

            return MatchOutcome.Draw;
        }

        // Returns the opposing disc color for the given player.
        private static CellState GetOpponentColor(CellState disc_color)
        {
            if (disc_color == CellState.Black)
            {
                return CellState.White;
            }

            return CellState.Black;
        }
    }
}
