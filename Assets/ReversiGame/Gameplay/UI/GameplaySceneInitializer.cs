using UnityEngine;
using Zenject;

namespace Game
{
    // This MonoBehaviour acts as the bridge between the gameplay scene and the match flow controller.
    // It reads the stored setup data, triggers match start, and reports completion back to the global flow.
    // This mirrors the BootstrapInitializeNotifier pattern so the gameplay scene's entry point is explicit.
    public class GameplaySceneInitializer : MonoBehaviour, IGameplaySceneController, IGameplayCoroutineRunner
    {
        private const bool IS_MISSING_SETUP_DATA_LOGGING_ENABLED = true;
        private const bool IS_MATCH_START_ERROR_LOGGING_ENABLED = true;

        private MatchFlowController _match_flow_controller;
        private IGameSetupSession _game_setup_session;
        private IGameplayCompletionAwaiter _gameplay_completion_awaiter;
        private IGameplaySceneRuntime _gameplay_scene_runtime;
        private IMatchResultSession _match_result_session;
        private IGameplayPostProcessingService _gameplay_post_processing_service;

        [Inject]
        private void Construct(
            MatchFlowController match_flow_controller,
            IGameSetupSession game_setup_session,
            IGameplayCompletionAwaiter gameplay_completion_awaiter,
            IGameplaySceneRuntime gameplay_scene_runtime,
            IMatchResultSession match_result_session,
            IGameplayPostProcessingService gameplay_post_processing_service)
        {
            _match_flow_controller = match_flow_controller;
            _game_setup_session = game_setup_session;
            _gameplay_completion_awaiter = gameplay_completion_awaiter;
            _gameplay_scene_runtime = gameplay_scene_runtime;
            _match_result_session = match_result_session;
            _gameplay_post_processing_service = gameplay_post_processing_service;
        }

        private void Awake()
        {
            _match_flow_controller.on_match_finished += HandleMatchFinished;
            _gameplay_scene_runtime.RegisterController(this);
            _gameplay_post_processing_service.ConfigureGameplayCanvas(FindFirstObjectByType<Canvas>());
        }

        // Reads setup data and starts or restarts the match on demand from the root gameplay state strategy.
        // Missing setup data is logged clearly and prevents an empty match from running.
        public void BeginMatch()
        {
            if (_match_flow_controller.match_phase == MatchPhase.InProgress)
            {
                return;
            }

            if (_game_setup_session.TryGetGameSetupData(out GameSetupData game_setup_data) == false)
            {
                GameDebugLogger.LogError(
                    "Gameplay scene started without valid setup data in the session.",
                    IS_MISSING_SETUP_DATA_LOGGING_ENABLED);
                return;
            }

            StartMatchSafely(game_setup_data);
        }

        public Coroutine StartGameplayCoroutine(System.Collections.IEnumerator routine)
        {
            return StartCoroutine(routine);
        }

        // Reports match completion to the global flow awaiter so the game manager can continue.
        private void HandleMatchFinished(MatchOutcome match_outcome)
        {
            _match_result_session.Store(match_outcome);
            _gameplay_completion_awaiter.NotifyGameplayCompleted();
        }

        private async void StartMatchSafely(GameSetupData game_setup_data)
        {
            try
            {
                await _match_flow_controller.StartMatchAsync(game_setup_data);
            }
            catch (System.Exception exception)
            {
                GameDebugLogger.LogError(
                    $"Gameplay match loop failed: {exception}",
                    IS_MATCH_START_ERROR_LOGGING_ENABLED);
            }
        }

        private void OnDestroy()
        {
            if (_gameplay_scene_runtime != null)
            {
                _gameplay_scene_runtime.UnregisterController(this);
            }

            if (_match_flow_controller != null)
            {
                _match_flow_controller.on_match_finished -= HandleMatchFinished;
            }
        }
    }
}
