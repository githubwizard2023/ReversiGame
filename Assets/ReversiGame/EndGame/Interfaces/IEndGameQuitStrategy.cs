using System.Threading.Tasks;

namespace Game
{
    // This interface represents one platform-specific reaction to the endgame quit button.
    // It uses a small strategy shape so platform behavior stays isolated instead of living in one switch-heavy view.
    // This fits the scene because some platforms quit immediately while others only show guidance.
    public interface IEndGameQuitStrategy
    {
        Platforms target_platform { get; }

        bool keeps_end_game_open { get; }

        Task ExecuteAsync();
    }
}
