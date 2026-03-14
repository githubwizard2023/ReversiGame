using System.Threading.Tasks;

namespace Game
{
    public class SplashContinueAwaiter : ISplashContinueAwaiter
    {
        private readonly object _sync_root = new object();
        private TaskCompletionSource<bool> _continue_request_source = CreateContinueRequestSource();

        public void Reset()
        {
            lock (_sync_root)
            {
                _continue_request_source = CreateContinueRequestSource();
            }
        }

        public Task WaitForContinueAsync()
        {
            lock (_sync_root)
            {
                return _continue_request_source.Task;
            }
        }

        public void NotifyContinueRequested()
        {
            TaskCompletionSource<bool> continue_request_source;

            lock (_sync_root)
            {
                continue_request_source = _continue_request_source;
            }

            continue_request_source.TrySetResult(true);
        }

        private static TaskCompletionSource<bool> CreateContinueRequestSource()
        {
            return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
