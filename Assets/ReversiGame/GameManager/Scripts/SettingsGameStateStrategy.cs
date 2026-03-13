using System.Threading.Tasks;

namespace Game
{
    // This strategy represents the dedicated game-settings stage after instructions.
    // It follows the same flow-strategy pattern so the manager can pause on the settings scene and resume on completion.
    // This fits the new scene split because the game manager should remain the only owner of gameplay handoff.
    public class SettingsGameStateStrategy : IGameFlowStateStrategy
    {
        private readonly IGameSetupCompletionAwaiter _game_setup_completion_awaiter;

        public SettingsGameStateStrategy(IGameSetupCompletionAwaiter game_setup_completion_awaiter)
        {
            _game_setup_completion_awaiter = game_setup_completion_awaiter;
        }

        public GameState game_state => GameState.Settings;

        // Waits for the settings screen to report completion, then returns the gameplay transition to the manager.
        public async Task<GameStateTransitionResult> ExecuteAsync()
        {
            _game_setup_completion_awaiter.Reset();
            await _game_setup_completion_awaiter.WaitForCompletionAsync();

            GameStateTransitionResult transition_result =
                new GameStateTransitionResult(GameState.Gameplay, GameSceneNames.GAMEPLAY_SCENE);

            return transition_result;
        }
    }
}
