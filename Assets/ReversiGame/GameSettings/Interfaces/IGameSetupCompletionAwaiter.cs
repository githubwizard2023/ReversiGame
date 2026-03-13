using System.Threading.Tasks;

namespace Game
{
    public interface IGameSetupCompletionAwaiter
    {
        void Reset();

        Task WaitForCompletionAsync();

        void NotifySetupCompleted();
    }
}
