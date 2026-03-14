using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    // This strategy represents the splash phase after the initial scene handoff.
    // It owns the startup delay before control moves to the instructions stage.
    // This keeps splash timing configurable without pushing wait logic into the manager.
    public class SplashGameStateStrategy : IGameFlowStateStrategy
    {
        private readonly GlobalGameSettings _global_game_settings;
        private readonly ISplashContinueAwaiter _splash_continue_awaiter;

        public SplashGameStateStrategy(
            GlobalGameSettings global_game_settings,
            ISplashContinueAwaiter splash_continue_awaiter)
        {
            _global_game_settings = global_game_settings;
            _splash_continue_awaiter = splash_continue_awaiter;
        }

        public GameState game_state => GameState.Splash;

        // Keeps the splash scene active for the configured duration before moving forward.
        // The manager handles the actual scene swap after this transition result returns.
        public async Task<GameStateTransitionResult> ExecuteAsync()
        {
            _splash_continue_awaiter.Reset();

            Task timeout_task = WaitForSplashTimeoutAsync();
            Task continue_task = _splash_continue_awaiter.WaitForContinueAsync();
            await Task.WhenAny(timeout_task, continue_task);

            GameStateTransitionResult transition_result =
                new GameStateTransitionResult(GameState.Instructions, GameSceneNames.INSTRUCTIONS_SCENE);

            return transition_result;
        }

        // Uses frame-based waiting so the splash timeout keeps working in WebGL builds.
        private async Task WaitForSplashTimeoutAsync()
        {
            float duration_seconds = Mathf.Max(0f, _global_game_settings.splash_duration_seconds);
            float end_time = Time.realtimeSinceStartup + duration_seconds;

            while (Time.realtimeSinceStartup < end_time)
            {
                await Task.Yield();
            }
        }
    }
}
