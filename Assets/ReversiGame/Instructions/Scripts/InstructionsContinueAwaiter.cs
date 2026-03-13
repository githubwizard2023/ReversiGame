using System.Threading.Tasks;

namespace Game
{
    // This class provides one shared await point for the instructions scene continue action.
    // It keeps the game-flow strategy decoupled from scene objects while still allowing a click-driven handoff.
    // This fits the current project because the instructions scene is simple and does not need its own installer.
    public class InstructionsContinueAwaiter : IInstructionsContinueAwaiter
    {
        private readonly object _sync_root = new object();
        private TaskCompletionSource<bool> _continue_request_source = CreateContinueRequestSource();

        // Resets the pending continue request before the instructions state starts waiting for a new click.
        public void Reset()
        {
            lock (_sync_root)
            {
                _continue_request_source = CreateContinueRequestSource();
            }
        }

        // Returns the current continue task so the flow strategy can await the next click.
        public Task WaitForContinueAsync()
        {
            lock (_sync_root)
            {
                return _continue_request_source.Task;
            }
        }

        // Completes the pending continue request when the instructions screen is clicked.
        public void NotifyContinueRequested()
        {
            TaskCompletionSource<bool> continue_request_source;

            lock (_sync_root)
            {
                continue_request_source = _continue_request_source;
            }

            continue_request_source.TrySetResult(true);
        }

        // Creates a task source that will resume the waiting strategy when the player continues.
        private static TaskCompletionSource<bool> CreateContinueRequestSource()
        {
            return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
