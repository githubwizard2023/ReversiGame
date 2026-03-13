using System.Threading.Tasks;

namespace Game
{
    public interface IInstructionsContinueAwaiter
    {
        void Reset();

        Task WaitForContinueAsync();

        void NotifyContinueRequested();
    }
}
