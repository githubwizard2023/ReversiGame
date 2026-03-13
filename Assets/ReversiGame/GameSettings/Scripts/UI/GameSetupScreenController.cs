using System.Threading.Tasks;

namespace Game
{
    // This class translates settings-screen selections into finalized match configuration and scene handoff.
    // It acts as the application-layer controller so storage and completion reporting stay outside the MonoBehaviour.
    // This fits the new flow because the settings scene should persist selection data and then return control to the game manager.
    public class GameSetupScreenController : IGameSetupScreenController
    {
        private readonly IGameSetupResolver _game_setup_resolver;
        private readonly IGameSetupSession _game_setup_session;
        private readonly IGameSetupCompletionAwaiter _game_setup_completion_awaiter;

        public GameSetupScreenController(
            IGameSetupResolver game_setup_resolver,
            IGameSetupSession game_setup_session,
            IGameSetupCompletionAwaiter game_setup_completion_awaiter)
        {
            _game_setup_resolver = game_setup_resolver;
            _game_setup_session = game_setup_session;
            _game_setup_completion_awaiter = game_setup_completion_awaiter;
        }

        // Resolves the player's selections, stores the final setup, and reports that the settings phase is complete.
        public Task StartGameAsync(GameSetupSelection game_setup_selection)
        {
            GameSetupData game_setup_data = _game_setup_resolver.Resolve(game_setup_selection);

            _game_setup_session.Store(game_setup_data);
            _game_setup_completion_awaiter.NotifySetupCompleted();

            return Task.CompletedTask;
        }
    }
}
