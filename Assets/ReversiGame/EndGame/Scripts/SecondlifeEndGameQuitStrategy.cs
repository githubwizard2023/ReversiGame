using System.Threading.Tasks;

namespace Game
{
    // This strategy handles quit requests for the custom Secondlife target setting.
    // It reuses the endgame result view so the platform-specific message stays in the current screen flow.
    // This fits the project because Secondlife needs custom feedback instead of a standard application quit.
    public class SecondlifeEndGameQuitStrategy : IEndGameQuitStrategy
    {
        private readonly EndGameResultView _end_game_result_view;

        public SecondlifeEndGameQuitStrategy(EndGameResultView end_game_result_view)
        {
            _end_game_result_view = end_game_result_view;
        }

        public Platforms target_platform => Platforms.Secondlife;

        public bool keeps_end_game_open => true;

        public Task ExecuteAsync()
        {
            return _end_game_result_view.ShowSecondlifeTextureAsync();
        }
    }
}
