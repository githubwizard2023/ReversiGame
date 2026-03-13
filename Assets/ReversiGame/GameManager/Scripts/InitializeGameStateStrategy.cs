using System.Threading.Tasks;

namespace Game
{
    // This strategy handles the initial boot phase before the first stage scene is shown.
    // It acts as the entry point in the table-driven flow and defines the first handoff.
    // This keeps startup decisions separate from the manager's orchestration code.
    public class InitializeGameStateStrategy : IGameFlowStateStrategy
    {
        public GameState game_state => GameState.Init;

        // Returns the first playable flow transition after startup initialization is complete.
        // The splash scene is loaded by the manager through the scene transition service.
        public Task<GameStateTransitionResult> ExecuteAsync()
        {
            GameStateTransitionResult transition_result =
                new GameStateTransitionResult(GameState.Splash, GameSceneNames.SPLASH_SCENE);

            return Task.FromResult(transition_result);
        }
    }
}
