using System.Threading.Tasks;

namespace Game
{
    public interface IEndGameChoiceAwaiter
    {
        void Reset();

        Task<EndGameChoice> WaitForChoiceAsync();

        void NotifyChoiceRequested(EndGameChoice end_game_choice);
    }
}
