using System;
using System.Threading.Tasks;

namespace Game
{
    // This strategy represents the splash phase after the initial scene handoff.
    // It owns the startup delay before control moves to the instructions stage.
    // This keeps splash timing configurable without pushing wait logic into the manager.
    public class SplashGameStateStrategy : IGameFlowStateStrategy
    {
        private readonly GlobalGameSettings _global_game_settings;

        public SplashGameStateStrategy(GlobalGameSettings global_game_settings)
        {
            _global_game_settings = global_game_settings;
        }

        public GameState game_state => GameState.Splash;

        // Keeps the splash scene active for the configured duration before moving forward.
        // The manager handles the actual scene swap after this transition result returns.
        public async Task<GameStateTransitionResult> ExecuteAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(_global_game_settings.splash_duration_seconds));

            GameStateTransitionResult transition_result =
                new GameStateTransitionResult(GameState.Instructions, GameSceneNames.INSTRUCTIONS_SCENE);

            return transition_result;
        }
    }
}
