using System.Threading.Tasks;

namespace Game
{
    // This interface defines the small boundary between the settings view and the setup application flow.
    // It follows the existing scene-controller pattern so the MonoBehaviour does not own storage or flow notification logic.
    // This fits the solution because the screen needs one focused async action: resolve settings and report completion.
    public interface IGameSetupScreenController
    {
        Task StartGameAsync(GameSetupSelection game_setup_selection);
    }
}
