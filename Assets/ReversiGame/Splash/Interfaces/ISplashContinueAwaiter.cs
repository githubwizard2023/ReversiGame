using System.Threading.Tasks;

namespace Game
{
    public interface ISplashContinueAwaiter
    {
        void Reset();

        Task WaitForContinueAsync();

        void NotifyContinueRequested();
    }
}
