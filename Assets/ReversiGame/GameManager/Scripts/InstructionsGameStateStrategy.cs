using System.Threading.Tasks;

namespace Game
{
    // This strategy represents the instructions phase after the splash handoff.
    // It waits for the player to acknowledge the instructions before advancing into the new settings scene.
    // This keeps the click-through behavior inside the flow state instead of scattering it across scenes.
    public class InstructionsGameStateStrategy : IGameFlowStateStrategy
    {
        private readonly IInstructionsContinueAwaiter _instructions_continue_awaiter;

        public InstructionsGameStateStrategy(IInstructionsContinueAwaiter instructions_continue_awaiter)
        {
            _instructions_continue_awaiter = instructions_continue_awaiter;
        }

        public GameState game_state => GameState.Instructions;

        // Waits for a click or touch press, then transitions into the dedicated settings screen.
        public async Task<GameStateTransitionResult> ExecuteAsync()
        {
            _instructions_continue_awaiter.Reset();
            await _instructions_continue_awaiter.WaitForContinueAsync();

            GameStateTransitionResult transition_result =
                new GameStateTransitionResult(GameState.Settings, GameSceneNames.GAME_SETTINGS_SCENE);

            return transition_result;
        }
    }
}
