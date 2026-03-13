using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game
{
    // This class orchestrates the turn-by-turn flow of a single Reversi match.
    // It coordinates domain services, AI, and match state without owning rendering or input.
    // This keeps the match loop in one focused controller and leaves UI interaction to callbacks.
    public class MatchFlowController
    {
        private const int AI_THINK_DELAY_MILLISECONDS = 400;

        private readonly ReversiBoard _board;
        private readonly ReversiLegalMoveFinder _legal_move_finder;
        private readonly ReversiMoveExecutor _move_executor;
        private readonly ReversiBoardEvaluator _board_evaluator;
        private readonly AIMoveChooser _ai_move_chooser;
        private readonly MatchState _match_state;

        private CellState _player_disc_color;
        private CellState _ai_disc_color;
        private DifficultyLevel _difficulty_level;
        private TaskCompletionSource<BoardPosition> _human_move_source;

        // Raised after each state change so the UI layer can refresh the board and HUD.
        public event Action on_board_changed;

        // Raised when a turn is passed so the UI can show a brief notification.
        public event Action<TurnParticipant> on_turn_passed;

        // Raised when the match ends so the UI can display the final result.
        public event Action<MatchOutcome> on_match_finished;

        // Raised when the active turn changes so the UI can update the turn indicator.
        public event Action<TurnParticipant> on_turn_changed;

        public TurnParticipant current_turn_participant => _match_state.current_turn_participant;

        public CellState player_disc_color => _player_disc_color;

        public CellState ai_disc_color => _ai_disc_color;

        public MatchPhase match_phase => _match_state.match_phase;

        public MatchFlowController(
            ReversiBoard board,
            ReversiLegalMoveFinder legal_move_finder,
            ReversiMoveExecutor move_executor,
            ReversiBoardEvaluator board_evaluator,
            AIMoveChooser ai_move_chooser,
            MatchState match_state)
        {
            _board = board;
            _legal_move_finder = legal_move_finder;
            _move_executor = move_executor;
            _board_evaluator = board_evaluator;
            _ai_move_chooser = ai_move_chooser;
            _match_state = match_state;
        }

        // Starts a new match using the resolved setup data from the settings phase.
        // Sets up the board, resolves colors, and begins the turn loop.
        public async Task StartMatchAsync(GameSetupData game_setup_data)
        {
            _player_disc_color = DiscColorToCellState(game_setup_data.player_disc_color);
            _ai_disc_color = DiscColorToCellState(game_setup_data.ai_disc_color);
            _difficulty_level = game_setup_data.difficulty_level;

            TurnParticipant starting_participant = ResolveStartingParticipant(game_setup_data.starting_participant);

            _board.SetupInitialPosition();
            _match_state.Initialize(starting_participant);

            on_board_changed?.Invoke();
            on_turn_changed?.Invoke(_match_state.current_turn_participant);

            await RunTurnLoopAsync();
        }

        // Called by the input router when the human player clicks a cell.
        // Resolves the pending human move source so the turn loop can continue.
        public void SubmitHumanMove(BoardPosition position)
        {
            if (_human_move_source == null)
            {
                return;
            }

            if (_match_state.current_turn_participant != TurnParticipant.Human)
            {
                return;
            }

            _human_move_source.TrySetResult(position);
        }

        // Returns the current legal moves for the human player so the UI can highlight them.
        public List<BoardPosition> GetCurrentLegalMoves()
        {
            if (_match_state.match_phase != MatchPhase.InProgress)
            {
                return new List<BoardPosition>();
            }

            if (_match_state.current_turn_participant != TurnParticipant.Human)
            {
                return new List<BoardPosition>();
            }

            return _legal_move_finder.FindLegalMoves(_board, _player_disc_color);
        }

        // Runs the alternating turn loop until the match reaches a terminal state.
        private async Task RunTurnLoopAsync()
        {
            while (_match_state.match_phase == MatchPhase.InProgress)
            {
                CellState current_disc_color = GetCurrentDiscColor();
                bool has_legal_moves = _legal_move_finder.HasAnyLegalMove(_board, current_disc_color);

                if (has_legal_moves == false)
                {
                    if (_match_state.did_previous_turn_pass)
                    {
                        EndMatch();
                        return;
                    }

                    on_turn_passed?.Invoke(_match_state.current_turn_participant);
                    _match_state.RecordPass();
                    on_turn_changed?.Invoke(_match_state.current_turn_participant);
                    continue;
                }

                if (_match_state.current_turn_participant == TurnParticipant.Human)
                {
                    await ExecuteHumanTurnAsync(current_disc_color);
                }
                else
                {
                    await ExecuteAITurnAsync(current_disc_color);
                }

                if (_board_evaluator.IsGameOver(_board))
                {
                    EndMatch();
                    return;
                }

                _match_state.RecordMove();
                on_turn_changed?.Invoke(_match_state.current_turn_participant);
            }
        }

        // Waits for the human player to submit a valid move through the input system.
        private async Task ExecuteHumanTurnAsync(CellState disc_color)
        {
            bool move_accepted = false;

            while (move_accepted == false)
            {
                _human_move_source = new TaskCompletionSource<BoardPosition>();
                on_board_changed?.Invoke();

                BoardPosition chosen_position = await _human_move_source.Task;
                _human_move_source = null;

                move_accepted = _move_executor.TryExecuteMove(
                    _board, chosen_position.row, chosen_position.column, disc_color);
            }

            on_board_changed?.Invoke();
        }

        // Lets the AI choose and execute its move with a small delay for visual pacing.
        private async Task ExecuteAITurnAsync(CellState disc_color)
        {
            on_board_changed?.Invoke();

            await Task.Delay(AI_THINK_DELAY_MILLISECONDS);

            BoardPosition? chosen_position = _ai_move_chooser.ChooseMove(_board, disc_color, _difficulty_level);

            if (chosen_position.HasValue)
            {
                _move_executor.TryExecuteMove(
                    _board, chosen_position.Value.row, chosen_position.Value.column, disc_color);
            }

            on_board_changed?.Invoke();
        }

        // Finalizes the match, records the outcome, and notifies listeners.
        private void EndMatch()
        {
            MatchOutcome outcome = _board_evaluator.DetermineOutcome(_board, _player_disc_color);
            _match_state.Finish(outcome);
            on_board_changed?.Invoke();
            on_match_finished?.Invoke(outcome);
        }

        // Returns the disc color for whoever's turn it currently is.
        private CellState GetCurrentDiscColor()
        {
            if (_match_state.current_turn_participant == TurnParticipant.Human)
            {
                return _player_disc_color;
            }

            return _ai_disc_color;
        }

        public CellState GetCurrentTurnDiscColor()
        {
            return GetCurrentDiscColor();
        }

        // Converts the setup-phase DiscColor enum to the gameplay-phase CellState enum.
        private static CellState DiscColorToCellState(DiscColor disc_color)
        {
            if (disc_color == DiscColor.Black)
            {
                return CellState.Black;
            }

            return CellState.White;
        }

        // Maps the setup-time starting participant to the runtime turn participant.
        private static TurnParticipant ResolveStartingParticipant(StartingParticipant starting_participant)
        {
            if (starting_participant == StartingParticipant.Human)
            {
                return TurnParticipant.Human;
            }

            return TurnParticipant.AI;
        }
    }
}
