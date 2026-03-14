using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_WEBGL || UNITY_EDITOR
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
#endif

namespace Game
{
    // This class selects the best move for the AI using minimax with alpha-beta pruning.
    // It uses a positional weight table tuned for classic 8x8 Reversi strategy.
    // On WebGL, AI computation is spread across frames via coroutine to avoid freezing the browser.
    public class AIMoveChooser
    {
        private const int BOARD_CELL_COUNT = ReversiBoard.BOARD_SIZE * ReversiBoard.BOARD_SIZE;
        private const int EASY_SEARCH_DEPTH   = 1;
        private const int MEDIUM_SEARCH_DEPTH = 3;
        private const int HARD_SEARCH_DEPTH   = 5;

        // WebGL depths: Medium/Hard get depth 2 now that work is spread across frames.
        private const int WEBGL_EASY_SEARCH_DEPTH   = 1;
        private const int WEBGL_MEDIUM_SEARCH_DEPTH = 2;
        private const int WEBGL_HARD_SEARCH_DEPTH   = 3;

        // How many minimax leaf evaluations to do per frame on WebGL before yielding.
        private const int WEBGL_WORK_BUDGET_PER_FRAME = 20;

        private const int INITIAL_ALPHA = int.MinValue;
        private const int INITIAL_BETA  = int.MaxValue;

        private readonly ReversiDiscFlipper      _disc_flipper;
        private readonly ReversiLegalMoveFinder  _legal_move_finder;
        private readonly ReversiMoveExecutor     _move_executor;

        // Shared counter so the coroutine knows when to yield.
        private int _webgl_work_this_frame;
        private int _webgl_completed_work_units;
        private int _webgl_counted_work_units;

        public AIMoveChooser(
            ReversiDiscFlipper disc_flipper,
            ReversiLegalMoveFinder legal_move_finder,
            ReversiMoveExecutor move_executor)
        {
            _disc_flipper       = disc_flipper;
            _legal_move_finder  = legal_move_finder;
            _move_executor      = move_executor;
        }

        // ─── Synchronous path (PC / Editor) ──────────────────────────────────────

        // Selects the best move for the AI based on the configured difficulty depth.
        // Returns the chosen position or null if no legal move exists.
        // Do NOT call this on WebGL builds — use ChooseMoveCoroutine instead.
        public BoardPosition? ChooseMove(
            ReversiBoard board, CellState ai_disc_color, DifficultyLevel difficulty_level)
        {
            int search_depth = ResolveSearchDepth(difficulty_level);
            return RunMinimax(board, ai_disc_color, search_depth);
        }

        // ─── Async / coroutine path (WebGL) ──────────────────────────────────────

        // Starts a non-blocking AI search spread across multiple frames.
        // The caller should use: StartCoroutine(ai.ChooseMoveCoroutine(board, color, diff, OnAIDone));
        // callback receives the chosen position (or null if no legal move).
        public IEnumerator ChooseMoveCoroutine(
            ReversiBoard board,
            CellState ai_disc_color,
            DifficultyLevel difficulty_level,
            Action<BoardPosition?> callback,
            Action<int, int> progress_callback = null)
        {
            int search_depth = ResolveSearchDepth(difficulty_level);
            CellState opponent_color = GetOpponentColor(ai_disc_color);
            int total_work_units = 0;
            _webgl_work_this_frame = 0;
            _webgl_counted_work_units = 0;
            progress_callback?.Invoke(0, 1);

            yield return CountSearchWorkUnitsCoroutine(
                board,
                search_depth,
                true,
                ai_disc_color,
                opponent_color,
                result => total_work_units = result,
                progress_callback);

            List<BoardPosition> legal_moves = _legal_move_finder.FindLegalMoves(board, ai_disc_color);

            if (legal_moves.Count == 0)
            {
                callback(null);
                yield break;
            }

            BoardPosition best_move  = legal_moves[0];
            int           best_score = INITIAL_ALPHA;

            _webgl_work_this_frame = 0;
            _webgl_completed_work_units = 0;
            progress_callback?.Invoke(0, total_work_units);

            foreach (BoardPosition move in legal_moves)
            {
                ReversiBoard cloned_board = board.Clone();
                _move_executor.TryExecuteMove(cloned_board, move.row, move.column, ai_disc_color);

                // Run the subtree; yields internally when budget is exceeded.
                int score = 0;
                yield return RunMinimaxCoroutine(
                    cloned_board, search_depth - 1, INITIAL_ALPHA, INITIAL_BETA,
                    false, ai_disc_color, opponent_color,
                    result => score = result,
                    progress_callback,
                    total_work_units);

                if (score > best_score)
                {
                    best_score = score;
                    best_move  = move;
                }

                // Always yield between top-level moves so the browser can breathe.
                progress_callback?.Invoke(_webgl_completed_work_units, total_work_units);
                yield return null;
                _webgl_work_this_frame = 0;
            }

            progress_callback?.Invoke(total_work_units, total_work_units);
            callback(best_move);
        }

        // ─── Core minimax (synchronous) ──────────────────────────────────────────

        private BoardPosition? RunMinimax(
            ReversiBoard board, CellState ai_disc_color, int search_depth)
        {
            CellState opponent_color = GetOpponentColor(ai_disc_color);
            List<BoardPosition> legal_moves = _legal_move_finder.FindLegalMoves(board, ai_disc_color);

            if (legal_moves.Count == 0) return null;

            BoardPosition best_move  = legal_moves[0];
            int           best_score = INITIAL_ALPHA;

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
                    best_move  = move;
                }
            }

            return best_move;
        }

        // Recursive minimax with alpha-beta pruning.
        private int Minimax(
            ReversiBoard board, int depth, int alpha, int beta,
            bool is_maximizing, CellState ai_disc_color, CellState opponent_color)
        {
            if (depth == 0)
                return EvaluateBoard(board, ai_disc_color, opponent_color);

            CellState current_color = is_maximizing ? ai_disc_color : opponent_color;
            List<BoardPosition> legal_moves = _legal_move_finder.FindLegalMoves(board, current_color);

            if (legal_moves.Count == 0)
            {
                CellState other_color       = is_maximizing ? opponent_color : ai_disc_color;
                bool      opponent_can_move = _legal_move_finder.HasAnyLegalMove(board, other_color);

                if (!opponent_can_move)
                    return EvaluateTerminalBoard(board, ai_disc_color, opponent_color);

                return Minimax(board, depth - 1, alpha, beta, !is_maximizing, ai_disc_color, opponent_color);
            }

            if (is_maximizing)
                return MaximizeScore(board, legal_moves, depth, alpha, beta, ai_disc_color, opponent_color);

            return MinimizeScore(board, legal_moves, depth, alpha, beta, ai_disc_color, opponent_color);
        }

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
                alpha     = Math.Max(alpha, score);
                if (beta <= alpha) break;
            }

            return max_score;
        }

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
                beta      = Math.Min(beta, score);
                if (beta <= alpha) break;
            }

            return min_score;
        }

        // ─── Coroutine minimax (WebGL only) ──────────────────────────────────────
        // Mirrors the synchronous minimax but yields when the per-frame budget is hit.

        private IEnumerator CountSearchWorkUnitsCoroutine(
            ReversiBoard board,
            int depth,
            bool is_maximizing,
            CellState ai_disc_color,
            CellState opponent_color,
            Action<int> result_callback,
            Action<int, int> progress_callback = null)
        {
            if (depth == 0)
            {
                _webgl_work_this_frame++;
                _webgl_counted_work_units++;
                progress_callback?.Invoke(_webgl_counted_work_units, _webgl_counted_work_units);

                if (_webgl_work_this_frame >= WEBGL_WORK_BUDGET_PER_FRAME)
                {
                    yield return null;
                    _webgl_work_this_frame = 0;
                }

                result_callback(1);
                yield break;
            }

            CellState current_color = is_maximizing ? ai_disc_color : opponent_color;
            List<BoardPosition> legal_moves = _legal_move_finder.FindLegalMoves(board, current_color);

            if (legal_moves.Count == 0)
            {
                CellState other_color = is_maximizing ? opponent_color : ai_disc_color;

                if (_legal_move_finder.HasAnyLegalMove(board, other_color) == false)
                {
                    _webgl_work_this_frame++;
                    _webgl_counted_work_units++;
                    progress_callback?.Invoke(_webgl_counted_work_units, _webgl_counted_work_units);

                    if (_webgl_work_this_frame >= WEBGL_WORK_BUDGET_PER_FRAME)
                    {
                        yield return null;
                        _webgl_work_this_frame = 0;
                    }

                    result_callback(1);
                    yield break;
                }

                int pass_count = 0;
                yield return CountSearchWorkUnitsCoroutine(
                    board,
                    depth - 1,
                    !is_maximizing,
                    ai_disc_color,
                    opponent_color,
                    result => pass_count = result,
                    progress_callback);

                result_callback(pass_count);
                yield break;
            }

            int total_count = 0;

            foreach (BoardPosition move in legal_moves)
            {
                ReversiBoard cloned_board = board.Clone();
                CellState mover = is_maximizing ? ai_disc_color : opponent_color;
                _move_executor.TryExecuteMove(cloned_board, move.row, move.column, mover);

                int branch_count = 0;
                yield return CountSearchWorkUnitsCoroutine(
                    cloned_board,
                    depth - 1,
                    !is_maximizing,
                    ai_disc_color,
                    opponent_color,
                    result => branch_count = result,
                    progress_callback);

                total_count += branch_count;
            }

            result_callback(total_count);
        }

        private IEnumerator RunMinimaxCoroutine(
            ReversiBoard board, int depth, int alpha, int beta,
            bool is_maximizing, CellState ai_disc_color, CellState opponent_color,
            Action<int> result_callback,
            Action<int, int> progress_callback = null,
            int total_work_units = 0)
        {
            if (depth == 0)
            {
                _webgl_work_this_frame++;
                _webgl_completed_work_units++;
                int leaf = EvaluateBoardManaged(board, ai_disc_color, opponent_color);

                if (_webgl_work_this_frame >= WEBGL_WORK_BUDGET_PER_FRAME)
                {
                    progress_callback?.Invoke(_webgl_completed_work_units, total_work_units);
                    yield return null;
                    _webgl_work_this_frame = 0;
                }

                result_callback(leaf);
                yield break;
            }

            CellState current_color = is_maximizing ? ai_disc_color : opponent_color;
            List<BoardPosition> legal_moves = _legal_move_finder.FindLegalMoves(board, current_color);

            if (legal_moves.Count == 0)
            {
                CellState other_color       = is_maximizing ? opponent_color : ai_disc_color;
                bool      opponent_can_move = _legal_move_finder.HasAnyLegalMove(board, other_color);

                if (!opponent_can_move)
                {
                    _webgl_completed_work_units++;
                    progress_callback?.Invoke(_webgl_completed_work_units, total_work_units);
                    result_callback(EvaluateTerminalBoardManaged(board, ai_disc_color, opponent_color));
                    yield break;
                }

                int pass_result = 0;
                yield return RunMinimaxCoroutine(
                    board, depth - 1, alpha, beta, !is_maximizing,
                    ai_disc_color, opponent_color, r => pass_result = r, progress_callback, total_work_units);

                result_callback(pass_result);
                yield break;
            }

            int  best    = is_maximizing ? INITIAL_ALPHA : INITIAL_BETA;
            bool pruned  = false;

            foreach (BoardPosition move in legal_moves)
            {
                if (pruned) break;

                ReversiBoard cloned_board = board.Clone();
                CellState    mover        = is_maximizing ? ai_disc_color : opponent_color;
                _move_executor.TryExecuteMove(cloned_board, move.row, move.column, mover);

                int child_score = 0;
                yield return RunMinimaxCoroutine(
                    cloned_board, depth - 1, alpha, beta, !is_maximizing,
                    ai_disc_color, opponent_color, r => child_score = r, progress_callback, total_work_units);

                if (is_maximizing)
                {
                    if (child_score > best) best = child_score;
                    if (child_score > alpha) alpha = child_score;
                }
                else
                {
                    if (child_score < best) best = child_score;
                    if (child_score < beta)  beta  = child_score;
                }

                if (beta <= alpha) pruned = true;
            }

            result_callback(best);
        }

        private static int EvaluateBoardManaged(
            ReversiBoard board, CellState ai_disc_color, CellState opponent_color)
        {
            int total_score = 0;

            for (int row = 0; row < ReversiBoard.BOARD_SIZE; row++)
            {
                for (int column = 0; column < ReversiBoard.BOARD_SIZE; column++)
                {
                    int       cell_index = (row * ReversiBoard.BOARD_SIZE) + column;
                    CellState cell       = board.GetCellState(row, column);
                    int       weight     = GetPositionWeight(cell_index);

                    if      (cell == ai_disc_color)   total_score += weight;
                    else if (cell == opponent_color)  total_score -= weight;
                }
            }

            return total_score;
        }

        private static int EvaluateTerminalBoardManaged(
            ReversiBoard board, CellState ai_disc_color, CellState opponent_color)
        {
            int ai_count = 0, opponent_count = 0;

            for (int row = 0; row < ReversiBoard.BOARD_SIZE; row++)
            {
                for (int column = 0; column < ReversiBoard.BOARD_SIZE; column++)
                {
                    CellState cell = board.GetCellState(row, column);
                    if      (cell == ai_disc_color)  ai_count++;
                    else if (cell == opponent_color) opponent_count++;
                }
            }

            if (ai_count > opponent_count)  return  10000 + (ai_count - opponent_count);
            if (opponent_count > ai_count)  return -10000 - (opponent_count - ai_count);
            return 0;
        }

        // ─── Evaluation entry points ─────────────────────────────────────────────

        private static int EvaluateBoard(
            ReversiBoard board, CellState ai_disc_color, CellState opponent_color)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return EvaluateBoardManaged(board, ai_disc_color, opponent_color);
#else
            NativeArray<byte> board_snapshot = CreateBoardSnapshot(board, Allocator.TempJob);
            NativeArray<int>  score          = new NativeArray<int>(1, Allocator.TempJob);

            try
            {
                WeightedBoardEvaluationJob job = new WeightedBoardEvaluationJob
                {
                    board_cells        = board_snapshot,
                    ai_disc_color      = (byte)ai_disc_color,
                    opponent_disc_color = (byte)opponent_color,
                    score              = score
                };
                job.Schedule().Complete();
                return score[0];
            }
            finally
            {
                if (score.IsCreated)          score.Dispose();
                if (board_snapshot.IsCreated) board_snapshot.Dispose();
            }
#endif
        }

        private static int EvaluateTerminalBoard(
            ReversiBoard board, CellState ai_disc_color, CellState opponent_color)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return EvaluateTerminalBoardManaged(board, ai_disc_color, opponent_color);
#else
            NativeArray<byte> board_snapshot = CreateBoardSnapshot(board, Allocator.TempJob);
            NativeArray<int>  score          = new NativeArray<int>(1, Allocator.TempJob);

            try
            {
                TerminalBoardEvaluationJob job = new TerminalBoardEvaluationJob
                {
                    board_cells         = board_snapshot,
                    ai_disc_color       = (byte)ai_disc_color,
                    opponent_disc_color = (byte)opponent_color,
                    score               = score
                };
                job.Schedule().Complete();
                return score[0];
            }
            finally
            {
                if (score.IsCreated)          score.Dispose();
                if (board_snapshot.IsCreated) board_snapshot.Dispose();
            }
#endif
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        private static int ResolveSearchDepth(DifficultyLevel difficulty_level)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            switch (difficulty_level)
            {
                case DifficultyLevel.Easy:   return WEBGL_EASY_SEARCH_DEPTH;
                case DifficultyLevel.Medium: return WEBGL_MEDIUM_SEARCH_DEPTH;
                case DifficultyLevel.Hard:   return WEBGL_HARD_SEARCH_DEPTH;
                default:                     return WEBGL_MEDIUM_SEARCH_DEPTH;
            }
#else
            switch (difficulty_level)
            {
                case DifficultyLevel.Easy:   return EASY_SEARCH_DEPTH;
                case DifficultyLevel.Medium: return MEDIUM_SEARCH_DEPTH;
                case DifficultyLevel.Hard:   return HARD_SEARCH_DEPTH;
                default:                     return MEDIUM_SEARCH_DEPTH;
            }
#endif
        }

        private static CellState GetOpponentColor(CellState disc_color)
            => disc_color == CellState.Black ? CellState.White : CellState.Black;

#if !UNITY_WEBGL || UNITY_EDITOR
        private static NativeArray<byte> CreateBoardSnapshot(ReversiBoard board, Allocator allocator)
        {
            NativeArray<byte> board_snapshot = new NativeArray<byte>(BOARD_CELL_COUNT, allocator);

            for (int row = 0; row < ReversiBoard.BOARD_SIZE; row++)
                for (int column = 0; column < ReversiBoard.BOARD_SIZE; column++)
                    board_snapshot[(row * ReversiBoard.BOARD_SIZE) + column] =
                        (byte)board.GetCellState(row, column);

            return board_snapshot;
        }
#endif

        private static int GetPositionWeight(int cell_index)
        {
            switch (cell_index)
            {
                case 0: case 7: case 56: case 63:
                    return 100;
                case 1: case 6: case 8: case 15:
                case 48: case 55: case 57: case 62:
                    return -20;
                case 2: case 5: case 16: case 23:
                case 40: case 47: case 58: case 61:
                    return 10;
                case 3: case 4: case 24: case 31:
                case 32: case 39: case 59: case 60:
                    return 8;
                case 9: case 14: case 49: case 54:
                    return -50;
                case 10: case 11: case 12: case 13:
                case 17: case 22: case 25: case 30:
                case 33: case 38: case 41: case 46:
                case 50: case 51: case 52: case 53:
                    return -2;
                case 18: case 21: case 42: case 45:
                    return 5;
                default:
                    return 1;
            }
        }

#if !UNITY_WEBGL || UNITY_EDITOR
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

                for (int i = 0; i < board_cells.Length; i++)
                {
                    byte cell   = board_cells[i];
                    int  weight = GetPositionWeight(i);

                    if      (cell == ai_disc_color)       total_score += weight;
                    else if (cell == opponent_disc_color) total_score -= weight;
                }

                score[0] = total_score;
            }
        }

        [BurstCompile]
        private struct TerminalBoardEvaluationJob : IJob
        {
            [ReadOnly] public NativeArray<byte> board_cells;
            public byte ai_disc_color;
            public byte opponent_disc_color;
            public NativeArray<int> score;

            public void Execute()
            {
                int ai_count = 0, opponent_count = 0;

                for (int i = 0; i < board_cells.Length; i++)
                {
                    byte cell = board_cells[i];
                    if      (cell == ai_disc_color)       ai_count++;
                    else if (cell == opponent_disc_color) opponent_count++;
                }

                if (ai_count > opponent_count)       { score[0] =  10000 + (ai_count - opponent_count); return; }
                if (opponent_count > ai_count)       { score[0] = -10000 - (opponent_count - ai_count);  return; }
                score[0] = 0;
            }
        }
#endif
    }
}
