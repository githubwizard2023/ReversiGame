using System.Threading.Tasks;

namespace Game
{
    // This interface defines one table entry in the game flow strategy map.
    // The manager resolves strategies by state instead of hard-coding transitions.
    // This keeps each phase focused on its own responsibility.
    public interface IGameFlowStateStrategy
    {
        GameState game_state { get; }

        // Executes the logic for the current state and returns the next transition instruction.
        // The manager uses the result to decide whether to load a new scene or continue the flow.
        Task<GameStateTransitionResult> ExecuteAsync();
    }
}
