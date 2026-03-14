using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    // Centralizes app quit behavior in the game-flow state machine.
    public class QuitGameStateStrategy : IGameFlowStateStrategy
    {
        private readonly IPlatformSelectionResolver _platform_selection_resolver;

        public QuitGameStateStrategy(IPlatformSelectionResolver platform_selection_resolver)
        {
            _platform_selection_resolver = platform_selection_resolver;
        }

        public GameState game_state => GameState.Quit;

        public Task<GameStateTransitionResult> ExecuteAsync()
        {
            switch (_platform_selection_resolver.ResolvePlatform())
            {
                case Platforms.EDITOR:
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;

                case Platforms.PC:
                    Application.Quit();
                    break;
            }

            GameStateTransitionResult transition_result =
                new GameStateTransitionResult(null, null);

            return Task.FromResult(transition_result);
        }
    }
}
