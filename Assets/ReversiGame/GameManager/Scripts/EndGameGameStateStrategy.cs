using System.Threading.Tasks;

namespace Game
{
    // Waits for the endgame scene to report which next action the player chose.
    public class EndGameGameStateStrategy : IGameFlowStateStrategy
    {
        private readonly IEndGameChoiceAwaiter _end_game_choice_awaiter;

        public EndGameGameStateStrategy(IEndGameChoiceAwaiter end_game_choice_awaiter)
        {
            _end_game_choice_awaiter = end_game_choice_awaiter;
        }

        public GameState game_state => GameState.EndGame;

        public async Task<GameStateTransitionResult> ExecuteAsync()
        {
            EndGameChoice end_game_choice = await _end_game_choice_awaiter.WaitForChoiceAsync();

            switch (end_game_choice)
            {
                case EndGameChoice.Menu:
                    return new GameStateTransitionResult(GameState.Settings, GameSceneNames.GAME_SETTINGS_SCENE);

                case EndGameChoice.PlayAgain:
                    return new GameStateTransitionResult(GameState.Gameplay, GameSceneNames.GAMEPLAY_SCENE);

                default:
                    return new GameStateTransitionResult(GameState.Quit, null);
            }
        }
    }
}
