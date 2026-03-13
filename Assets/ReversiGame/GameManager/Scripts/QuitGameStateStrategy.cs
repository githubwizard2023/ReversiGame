using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    // Centralizes app quit behavior in the game-flow state machine.
    public class QuitGameStateStrategy : IGameFlowStateStrategy
    {
        public GameState game_state => GameState.Quit;

        public Task<GameStateTransitionResult> ExecuteAsync()
        {
            Application.Quit();

            GameStateTransitionResult transition_result =
                new GameStateTransitionResult(null, null);

            return Task.FromResult(transition_result);
        }
    }
}
