using System.Threading.Tasks;

namespace Game
{
    // Carries the endgame button choice back into the game-flow state machine.
    public class EndGameChoiceAwaiter : IEndGameChoiceAwaiter
    {
        private readonly object _sync_root = new object();
        private TaskCompletionSource<EndGameChoice> _choice_source = CreateChoiceSource();

        public void Reset()
        {
            lock (_sync_root)
            {
                _choice_source = CreateChoiceSource();
            }
        }

        public Task<EndGameChoice> WaitForChoiceAsync()
        {
            lock (_sync_root)
            {
                return _choice_source.Task;
            }
        }

        public void NotifyChoiceRequested(EndGameChoice end_game_choice)
        {
            TaskCompletionSource<EndGameChoice> choice_source;

            lock (_sync_root)
            {
                choice_source = _choice_source;
            }

            choice_source.TrySetResult(end_game_choice);
        }

        private static TaskCompletionSource<EndGameChoice> CreateChoiceSource()
        {
            return new TaskCompletionSource<EndGameChoice>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
