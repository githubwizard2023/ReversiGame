using UnityEngine;
using Zenject;

namespace Game
{
    // This MonoBehaviour acts as the bridge between the gameplay scene and the match flow controller.
    // It reads the stored setup data, triggers match start, and reports completion back to the global flow.
    // This mirrors the BootstrapInitializeNotifier pattern so the gameplay scene's entry point is explicit.
    public class GameplaySceneInitializer : MonoBehaviour
    {
        private const bool IS_MISSING_SETUP_DATA_LOGGING_ENABLED = true;

        private MatchFlowController _match_flow_controller;
        private IGameSetupSession _game_setup_session;
        private IGameplayCompletionAwaiter _gameplay_completion_awaiter;
        private IMatchResultSession _match_result_session;

        [Inject]
        private void Construct(
            MatchFlowController match_flow_controller,
            IGameSetupSession game_setup_session,
            IGameplayCompletionAwaiter gameplay_completion_awaiter,
            IMatchResultSession match_result_session)
        {
            _match_flow_controller = match_flow_controller;
            _game_setup_session = game_setup_session;
            _gameplay_completion_awaiter = gameplay_completion_awaiter;
            _match_result_session = match_result_session;
        }

        // Reads setup data and starts the match once the gameplay scene is active.
        // Missing setup data is logged clearly and prevents an empty match from running.
        private void Start()
        {
            if (_game_setup_session.TryGetGameSetupData(out GameSetupData game_setup_data) == false)
            {
                GameDebugLogger.LogError(
                    "Gameplay scene started without valid setup data in the session.",
                    IS_MISSING_SETUP_DATA_LOGGING_ENABLED);
                return;
            }

            _match_flow_controller.on_match_finished += HandleMatchFinished;
            _ = _match_flow_controller.StartMatchAsync(game_setup_data);
        }

        // Reports match completion to the global flow awaiter so the game manager can continue.
        private void HandleMatchFinished(MatchOutcome match_outcome)
        {
            _match_result_session.Store(match_outcome);
            _gameplay_completion_awaiter.NotifyGameplayCompleted();
        }

        private void OnDestroy()
        {
            if (_match_flow_controller != null)
            {
                _match_flow_controller.on_match_finished -= HandleMatchFinished;
            }
        }
    }
}
