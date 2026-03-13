using System.Threading.Tasks;

namespace Game
{
    // This interface abstracts the gameplay completion signal away from the strategy.
    // It follows the same awaiter pattern used by IGameSetupCompletionAwaiter.
    // This keeps the game flow strategy testable and decoupled from gameplay scene internals.
    public interface IGameplayCompletionAwaiter
    {
        // Resets the awaiter so it can be used for a new gameplay session.
        void Reset();

        // Blocks the calling strategy until the gameplay scene reports that the match is over.
        Task WaitForCompletionAsync();

        // Called by the gameplay scene when the match has ended.
        void NotifyGameplayCompleted();
    }
}
