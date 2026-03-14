using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game
{
    // This class is the root orchestrator for the game's high-level flow.
    // It uses a table-driven strategy map so each phase can define its own transition logic.
    // This keeps flow control centralized without turning the manager into a giant switch statement.
    public class GameManager : IGameManager
    {
        private const bool IS_BOOTSTRAP_REPEAT_LOGGING_ENABLED = false;
        private const bool IS_MISSING_STATE_STRATEGY_LOGGING_ENABLED = false;
        private const bool IS_DUPLICATE_STATE_STRATEGY_LOGGING_ENABLED = false;

        private readonly Dictionary<GameState, IGameFlowStateStrategy> _state_strategies;
        private readonly IGameSceneTransitionService _game_scene_transition_service;
        private bool _has_bootstrap_initialize_completed;

        public GameManager(
            List<IGameFlowStateStrategy> state_strategies,
            IGameSceneTransitionService game_scene_transition_service)
        {
            _state_strategies = BuildStateStrategyTable(state_strategies);
            _game_scene_transition_service = game_scene_transition_service;
        }

        // Starts the game flow only after the bootstrap scene reports that initialization is complete.
        // Repeated notifications are ignored so startup cannot run twice by accident.
        public void NotifyBootstrapInitializeCompleted()
        {
            if (_has_bootstrap_initialize_completed)
            {
                GameDebugLogger.LogWarning(
                    "Bootstrap initialization was already completed.",
                    IS_BOOTSTRAP_REPEAT_LOGGING_ENABLED);
                return;
            }

            _has_bootstrap_initialize_completed = true;
            _ = RunGameFlowAsync(GameState.Init);
        }

        // Executes the state table in sequence and applies scene transitions requested by strategies.
        // Missing strategies stop the flow and log an error instead of failing silently.
        private async Task RunGameFlowAsync(GameState initial_game_state)
        {
            GameState? current_game_state = initial_game_state;

            while (current_game_state.HasValue)
            {
                GameState active_game_state = current_game_state.Value;

                if (_state_strategies.TryGetValue(active_game_state, out IGameFlowStateStrategy state_strategy) == false)
                {
                    GameDebugLogger.LogError(
                        $"Missing game flow strategy for state: {active_game_state}",
                        IS_MISSING_STATE_STRATEGY_LOGGING_ENABLED);
                    return;
                }

                GameStateTransitionResult transition_result = await state_strategy.ExecuteAsync();

                foreach (string scene_name_to_unload in transition_result.scenes_to_unload)
                {
                    await _game_scene_transition_service.UnloadSceneAsync(scene_name_to_unload);
                }

                if (string.IsNullOrWhiteSpace(transition_result.target_scene_name) == false)
                {
                    if (transition_result.should_replace_current_stage_scene)
                    {
                        await _game_scene_transition_service.TransitionToStageSceneAsync(
                            transition_result.target_scene_name);
                    }
                    else
                    {
                        await _game_scene_transition_service.LoadSceneAdditivelyAsync(
                            transition_result.target_scene_name);
                    }
                }

                current_game_state = transition_result.next_game_state;
            }
        }

        // Builds the state lookup table once so runtime flow resolution stays simple and fast.
        // Duplicate state registrations are treated as configuration errors.
        private static Dictionary<GameState, IGameFlowStateStrategy> BuildStateStrategyTable(
            IEnumerable<IGameFlowStateStrategy> state_strategies)
        {
            Dictionary<GameState, IGameFlowStateStrategy> state_strategy_table =
                new Dictionary<GameState, IGameFlowStateStrategy>();

            foreach (IGameFlowStateStrategy state_strategy in state_strategies)
            {
                if (state_strategy_table.ContainsKey(state_strategy.game_state))
                {
                    GameDebugLogger.LogError(
                        $"Duplicate game flow strategy found for state: {state_strategy.game_state}",
                        IS_DUPLICATE_STATE_STRATEGY_LOGGING_ENABLED);
                    continue;
                }

                state_strategy_table.Add(state_strategy.game_state, state_strategy);
            }

            return state_strategy_table;
        }
    }
}
