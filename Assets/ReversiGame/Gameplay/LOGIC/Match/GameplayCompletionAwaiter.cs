using System.Threading.Tasks;

namespace Game
{
    // This class provides a task-based completion signal for the gameplay phase.
    // It mirrors the GameSetupCompletionAwaiter so the game manager can pause on gameplay the same way.
    // This keeps the awaiter pattern consistent across all flow phases.
    public class GameplayCompletionAwaiter : IGameplayCompletionAwaiter
    {
        private readonly object _sync_root = new object();
        private TaskCompletionSource<bool> _completion_source = CreateCompletionSource();

        // Prepares a fresh task source so the next wait call gets a pending task.
        public void Reset()
        {
            lock (_sync_root)
            {
                _completion_source = CreateCompletionSource();
            }
        }

        // Returns the current completion task so the gameplay strategy can await match end.
        public Task WaitForCompletionAsync()
        {
            lock (_sync_root)
            {
                return _completion_source.Task;
            }
        }

        // Resolves the pending task so the game flow strategy can continue to the next state.
        public void NotifyGameplayCompleted()
        {
            TaskCompletionSource<bool> completion_source;

            lock (_sync_root)
            {
                completion_source = _completion_source;
            }

            completion_source.TrySetResult(true);
        }

        // Creates a task source that resumes the waiting gameplay strategy once the match ends.
        private static TaskCompletionSource<bool> CreateCompletionSource()
        {
            return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
