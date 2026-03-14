using System.Threading.Tasks;

namespace Game
{
    // This strategy represents the gameplay phase after the settings flow completes.
    // It follows the same table-driven pattern so the manager can pause on gameplay and resume on match end.
    // This keeps the gameplay lifecycle managed by the same flow mechanism as all other game states.
    public class GameplayGameStateStrategy : IGameFlowStateStrategy
    {
        private readonly IGameplayCompletionAwaiter _gameplay_completion_awaiter;
        private readonly IGameplaySceneRuntime _gameplay_scene_runtime;
        private readonly IMatchResultSession _match_result_session;
        private readonly IEndGameChoiceAwaiter _end_game_choice_awaiter;
        private readonly IGameplayPostProcessingService _gameplay_post_processing_service;

        public GameplayGameStateStrategy(
            IGameplayCompletionAwaiter gameplay_completion_awaiter,
            IGameplaySceneRuntime gameplay_scene_runtime,
            IMatchResultSession match_result_session,
            IEndGameChoiceAwaiter end_game_choice_awaiter,
            IGameplayPostProcessingService gameplay_post_processing_service)
        {
            _gameplay_completion_awaiter = gameplay_completion_awaiter;
            _gameplay_scene_runtime = gameplay_scene_runtime;
            _match_result_session = match_result_session;
            _end_game_choice_awaiter = end_game_choice_awaiter;
            _gameplay_post_processing_service = gameplay_post_processing_service;
        }

        public GameState game_state => GameState.Gameplay;

        // Waits for the gameplay scene to report that the match has ended, then transitions to the endgame state.
        public async Task<GameStateTransitionResult> ExecuteAsync()
        {
            _gameplay_post_processing_service.SetEndGameBlurEnabled(false);
            _match_result_session.Clear();
            _gameplay_completion_awaiter.Reset();
            await _gameplay_scene_runtime.BeginMatchAsync();
            await _gameplay_completion_awaiter.WaitForCompletionAsync();
            _end_game_choice_awaiter.Reset();
            _gameplay_post_processing_service.SetEndGameBlurEnabled(true);

            GameStateTransitionResult transition_result =
                new GameStateTransitionResult(GameState.EndGame, GameSceneNames.END_GAME_SCENE, false, null);

            return transition_result;
        }
    }
}
