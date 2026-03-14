using System.Threading.Tasks;

namespace Game
{
    // This strategy handles quit requests for WebGL where the application cannot close the browser tab.
    // It uses the endgame result view as a lightweight presentation boundary for temporary guidance.
    // This fits the project because browser quit needs feedback rather than a real application exit.
    public class WebGlEndGameQuitStrategy : IEndGameQuitStrategy
    {
        private readonly EndGameResultView _end_game_result_view;

        public WebGlEndGameQuitStrategy(EndGameResultView end_game_result_view)
        {
            _end_game_result_view = end_game_result_view;
        }

        public Platforms target_platform => Platforms.WebGL;

        public bool keeps_end_game_open => true;

        public Task ExecuteAsync()
        {
            return _end_game_result_view.ShowCloseTabTextureAsync();
        }
    }
}
