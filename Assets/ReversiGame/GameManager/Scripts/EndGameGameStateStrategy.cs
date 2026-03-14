using System.Threading.Tasks;

namespace Game
{
    // Waits for the endgame scene to report which next action the player chose.
    public class EndGameGameStateStrategy : IGameFlowStateStrategy
    {
        private readonly IEndGameChoiceAwaiter _end_game_choice_awaiter;
        private readonly IGameplayPostProcessingService _gameplay_post_processing_service;

        public EndGameGameStateStrategy(
            IEndGameChoiceAwaiter end_game_choice_awaiter,
            IGameplayPostProcessingService gameplay_post_processing_service)
        {
            _end_game_choice_awaiter = end_game_choice_awaiter;
            _gameplay_post_processing_service = gameplay_post_processing_service;
        }

        public GameState game_state => GameState.EndGame;

        public async Task<GameStateTransitionResult> ExecuteAsync()
        {
            EndGameChoice end_game_choice = await _end_game_choice_awaiter.WaitForChoiceAsync();
            _gameplay_post_processing_service.SetEndGameBlurEnabled(false);

            switch (end_game_choice)
            {
                case EndGameChoice.Menu:
                    return new GameStateTransitionResult(
                        GameState.Settings,
                        GameSceneNames.GAME_SETTINGS_SCENE,
                        true,
                        new[]
                        {
                            GameSceneNames.END_GAME_SCENE,
                            GameSceneNames.GAMEPLAY_SCENE
                        });

                case EndGameChoice.PlayAgain:
                    return new GameStateTransitionResult(
                        GameState.Gameplay,
                        null,
                        true,
                        new[]
                        {
                            GameSceneNames.END_GAME_SCENE
                        });

                default:
                    return new GameStateTransitionResult(GameState.Quit, null);
            }
        }
    }
}
