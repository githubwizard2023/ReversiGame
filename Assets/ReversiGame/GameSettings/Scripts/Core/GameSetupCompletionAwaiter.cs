using System.Threading.Tasks;

namespace Game
{
    // This class provides one shared await point for the settings scene completion action.
    // It follows the same awaiter pattern as the instructions flow so scene UI reports completion without owning transitions.
    // This fits the project because the game manager should remain the only owner of state progression and scene changes.
    public class GameSetupCompletionAwaiter : IGameSetupCompletionAwaiter
    {
        private readonly object _sync_root = new object();
        private TaskCompletionSource<bool> _completion_source = CreateCompletionSource();

        // Resets the pending setup-complete request before the settings state starts waiting for a new start action.
        public void Reset()
        {
            lock (_sync_root)
            {
                _completion_source = CreateCompletionSource();
            }
        }

        // Returns the current completion task so the settings flow can await the next confirmed start action.
        public Task WaitForCompletionAsync()
        {
            lock (_sync_root)
            {
                return _completion_source.Task;
            }
        }

        // Completes the pending setup request when the settings screen has stored the chosen match configuration.
        public void NotifySetupCompleted()
        {
            TaskCompletionSource<bool> completion_source;

            lock (_sync_root)
            {
                completion_source = _completion_source;
            }

            completion_source.TrySetResult(true);
        }

        // Creates a task source that resumes the waiting settings strategy once the player starts the match.
        private static TaskCompletionSource<bool> CreateCompletionSource()
        {
            return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
