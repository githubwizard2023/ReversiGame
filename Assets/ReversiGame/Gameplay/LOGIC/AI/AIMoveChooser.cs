using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Game
{
    // This class selects the best move for the AI using minimax with alpha-beta pruning.
    // It uses a positional weight table tuned for classic 8x8 Reversi strategy.
    // This keeps AI decision-making isolated from match flow and board rendering.
    public class AIMoveChooser
    {
        private const int BOARD_CELL_COUNT = ReversiBoard.BOARD_SIZE * ReversiBoard.BOARD_SIZE;
        private const int EASY_SEARCH_DEPTH = 1;
        private const int MEDIUM_SEARCH_DEPTH = 3;
        private const int HARD_SEARCH_DEPTH = 5;

        private const int INITIAL_ALPHA = int.MinValue;
        private const int INITIAL_BETA = int.MaxValue;

        private readonly ReversiDiscFlipper _disc_flipper;
        private readonly ReversiLegalMoveFinder _legal_move_finder;
        private readonly ReversiMoveExecutor _move_executor;

        public AIMoveChooser(
            ReversiDiscFlipper disc_flipper,
            ReversiLegalMoveFinder legal_move_finder,
            ReversiMoveExecutor move_executor)
        {
            _disc_flipper = disc_flipper;
            _legal_move_finder = legal_move_finder;
            _move_executor = move_executor;
        }

        // Selects the best move for the AI based on the configured difficulty depth.
        // Returns the chosen position or null if no legal move exists.
        public BoardPosition? ChooseMove(ReversiBoard board, CellState ai_disc_color, DifficultyLevel difficulty_level)
        {
            int search_depth = ResolveSearchDepth(difficulty_level);
            CellState opponent_color = GetOpponentColor(ai_disc_color);

            List<BoardPosition> legal_moves = _legal_move_finder.FindLegalMoves(board, ai_disc_color);

            if (legal_moves.Count == 0)
            {
                return null;
            }

            BoardPosition best_move = legal_moves[0];
            int best_score = INITIAL_ALPHA;

            foreach (BoardPosition move in legal_moves)
            {
                ReversiBoard cloned_board = board.Clone();
                _move_executor.TryExecuteMove(cloned_board, move.row, move.column, ai_disc_color);

                int score = Minimax(
                    cloned_board, search_depth - 1, INITIAL_ALPHA, INITIAL_BETA,
                    false, ai_disc_color, opponent_color);

                if (score > best_score)
                {
                    best_score = score;
                    best_move = move;
                }
            }

            return best_move;
        }

        // Recursive minimax with alpha-beta pruning.
        // The maximizing player is always the AI; the minimizing player is the opponent.
        private int Minimax(
            ReversiBoard board, int depth, int alpha, int beta,
            bool is_maximizing, CellState ai_disc_color, CellState opponent_color)
        {
            if (depth == 0)
            {
                return EvaluateBoard(board, ai_disc_color, opponent_color);
            }

            CellState current_color = is_maximizing ? ai_disc_color : opponent_color;
            List<BoardPosition> legal_moves = _legal_move_finder.FindLegalMoves(board, current_color);

            if (legal_moves.Count == 0)
            {
                CellState other_color = is_maximizing ? opponent_color : ai_disc_color;
                bool opponent_can_move = _legal_move_finder.HasAnyLegalMove(board, other_color);

                if (opponent_can_move == false)
                {
                    return EvaluateTerminalBoard(board, ai_disc_color, opponent_color);
                }

                return Minimax(board, depth - 1, alpha, beta, !is_maximizing, ai_disc_color, opponent_color);
            }

            if (is_maximizing)
            {
                return MaximizeScore(board, legal_moves, depth, alpha, beta, ai_disc_color, opponent_color);
            }

            return MinimizeScore(board, legal_moves, depth, alpha, beta, ai_disc_color, opponent_color);
        }

        // Explores all moves for the maximizing player and applies alpha-beta cutoffs.
        private int MaximizeScore(
            ReversiBoard board, List<BoardPosition> legal_moves,
            int depth, int alpha, int beta,
            CellState ai_disc_color, CellState opponent_color)
        {
            int max_score = INITIAL_ALPHA;

            foreach (BoardPosition move in legal_moves)
            {
                ReversiBoard cloned_board = board.Clone();
                _move_executor.TryExecuteMove(cloned_board, move.row, move.column, ai_disc_color);

                int score = Minimax(cloned_board, depth - 1, alpha, beta, false, ai_disc_color, opponent_color);
                max_score = Math.Max(max_score, score);
                alpha = Math.Max(alpha, score);

                if (beta <= alpha)
                {
                    break;
                }
            }

            return max_score;
        }

        // Explores all moves for the minimizing player and applies alpha-beta cutoffs.
        private int MinimizeScore(
            ReversiBoard board, List<BoardPosition> legal_moves,
            int depth, int alpha, int beta,
            CellState ai_disc_color, CellState opponent_color)
        {
            int min_score = INITIAL_BETA;

            foreach (BoardPosition move in legal_moves)
            {
                ReversiBoard cloned_board = board.Clone();
                _move_executor.TryExecuteMove(cloned_board, move.row, move.column, opponent_color);

                int score = Minimax(cloned_board, depth - 1, alpha, beta, true, ai_disc_color, opponent_color);
                min_score = Math.Min(min_score, score);
                beta = Math.Min(beta, score);

                if (beta <= alpha)
                {
                    break;
                }
            }

            return min_score;
        }

        // Evaluates a non-terminal board using positional weights biased toward the AI.
        private static int EvaluateBoard(ReversiBoard board, CellState ai_disc_color, CellState opponent_color)
        {
            NativeArray<byte> board_snapshot = CreateBoardSnapshot(board, Allocator.TempJob);
            NativeArray<int> score = new NativeArray<int>(1, Allocator.TempJob);

            try
            {
                WeightedBoardEvaluationJob evaluation_job = new WeightedBoardEvaluationJob
                {
                    board_cells = board_snapshot,
                    ai_disc_color = (byte)ai_disc_color,
                    opponent_disc_color = (byte)opponent_color,
                    score = score
                };

                JobHandle handle = evaluation_job.Schedule();
                handle.Complete();

                return score[0];
            }
            finally
            {
                if (score.IsCreated)
                {
                    score.Dispose();
                }

                if (board_snapshot.IsCreated)
                {
                    board_snapshot.Dispose();
                }
            }
        }

        // Evaluates a terminal board with a large bonus or penalty based on the final disc count.
        private static int EvaluateTerminalBoard(
            ReversiBoard board, CellState ai_disc_color, CellState opponent_color)
        {
            NativeArray<byte> board_snapshot = CreateBoardSnapshot(board, Allocator.TempJob);
            NativeArray<int> score = new NativeArray<int>(1, Allocator.TempJob);

            try
            {
                TerminalBoardEvaluationJob evaluation_job = new TerminalBoardEvaluationJob
                {
                    board_cells = board_snapshot,
                    ai_disc_color = (byte)ai_disc_color,
                    opponent_disc_color = (byte)opponent_color,
                    score = score
                };

                JobHandle handle = evaluation_job.Schedule();
                handle.Complete();

                return score[0];
            }
            finally
            {
                if (score.IsCreated)
                {
                    score.Dispose();
                }

                if (board_snapshot.IsCreated)
                {
                    board_snapshot.Dispose();
                }
            }
        }

        // Maps the difficulty setting to the minimax search depth.
        private static int ResolveSearchDepth(DifficultyLevel difficulty_level)
        {
            switch (difficulty_level)
            {
                case DifficultyLevel.Easy:
                    return EASY_SEARCH_DEPTH;

                case DifficultyLevel.Medium:
                    return MEDIUM_SEARCH_DEPTH;

                case DifficultyLevel.Hard:
                    return HARD_SEARCH_DEPTH;

                default:
                    return MEDIUM_SEARCH_DEPTH;
            }
        }

        // Returns the opposing disc color.
        private static CellState GetOpponentColor(CellState disc_color)
        {
            if (disc_color == CellState.Black)
            {
                return CellState.White;
            }

            return CellState.Black;
        }

        // Copies the managed board into a native flat buffer so Burst jobs can evaluate it safely.
        private static NativeArray<byte> CreateBoardSnapshot(ReversiBoard board, Allocator allocator)
        {
            NativeArray<byte> board_snapshot = new NativeArray<byte>(BOARD_CELL_COUNT, allocator);

            for (int row = 0; row < ReversiBoard.BOARD_SIZE; row++)
            {
                for (int column = 0; column < ReversiBoard.BOARD_SIZE; column++)
                {
                    int cell_index = (row * ReversiBoard.BOARD_SIZE) + column;
                    board_snapshot[cell_index] = (byte)board.GetCellState(row, column);
                }
            }

            return board_snapshot;
        }

        // Resolves the positional heuristic weight for a flattened board index without managed array access.
        private static int GetPositionWeight(int cell_index)
        {
            switch (cell_index)
            {
                case 0:
                case 7:
                case 56:
                case 63:
                    return 100;

                case 1:
                case 6:
                case 8:
                case 15:
                case 48:
                case 55:
                case 57:
                case 62:
                    return -20;

                case 2:
                case 5:
                case 16:
                case 23:
                case 40:
                case 47:
                case 58:
                case 61:
                    return 10;

                case 3:
                case 4:
                case 24:
                case 31:
                case 32:
                case 39:
                case 59:
                case 60:
                    return 8;

                case 9:
                case 14:
                case 49:
                case 54:
                    return -50;

                case 10:
                case 11:
                case 12:
                case 13:
                case 17:
                case 22:
                case 25:
                case 30:
                case 33:
                case 38:
                case 41:
                case 46:
                case 50:
                case 51:
                case 52:
                case 53:
                    return -2;

                case 18:
                case 21:
                case 42:
                case 45:
                    return 5;

                default:
                    return 1;
            }
        }

        // This Burst job evaluates positional control on a board snapshot without touching managed objects.
        // It acts as a data-oriented leaf scorer for minimax while preserving the existing orchestration layer.
        // This fits the current solution because only the hot scoring loop needs native optimization.
        [BurstCompile]
        private struct WeightedBoardEvaluationJob : IJob
        {
            [ReadOnly] public NativeArray<byte> board_cells;
            public byte ai_disc_color;
            public byte opponent_disc_color;
            public NativeArray<int> score;

            public void Execute()
            {
                int total_score = 0;

                for (int cell_index = 0; cell_index < board_cells.Length; cell_index++)
                {
                    byte cell = board_cells[cell_index];
                    int weight = GetPositionWeight(cell_index);

                    if (cell == ai_disc_color)
                    {
                        total_score += weight;
                    }
                    else if (cell == opponent_disc_color)
                    {
                        total_score -= weight;
                    }
                }

                score[0] = total_score;
            }
        }

        // This Burst job computes the terminal win-loss score from a board snapshot and final disc counts.
        // It keeps the terminal evaluation branch native even though move generation remains object-based.
        // This fits the current solution because terminal scoring is deterministic and easy to flatten.
        [BurstCompile]
        private struct TerminalBoardEvaluationJob : IJob
        {
            [ReadOnly] public NativeArray<byte> board_cells;
            public byte ai_disc_color;
            public byte opponent_disc_color;
            public NativeArray<int> score;

            public void Execute()
            {
                int ai_count = 0;
                int opponent_count = 0;

                for (int cell_index = 0; cell_index < board_cells.Length; cell_index++)
                {
                    byte cell = board_cells[cell_index];

                    if (cell == ai_disc_color)
                    {
                        ai_count++;
                    }
                    else if (cell == opponent_disc_color)
                    {
                        opponent_count++;
                    }
                }

                if (ai_count > opponent_count)
                {
                    score[0] = 10000 + (ai_count - opponent_count);
                    return;
                }

                if (opponent_count > ai_count)
                {
                    score[0] = -10000 - (opponent_count - ai_count);
                    return;
                }

                score[0] = 0;
            }
        }
    }
}
