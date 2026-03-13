namespace Game
{
    // This class stores the latest resolved setup so the gameplay flow can consume it after the setup screen closes.
    // It acts as a small scene-level state holder instead of pushing transient setup data into UI components.
    // This keeps the selected match configuration available without introducing a wider session framework.
    public class GameSetupSession : IGameSetupSession
    {
        private GameSetupData? _game_setup_data;

        // Stores the final resolved setup data that should be used when gameplay begins.
        public void Store(GameSetupData game_setup_data)
        {
            _game_setup_data = game_setup_data;
        }

        // Returns the stored setup if the player has already started a match from the setup screen.
        public bool TryGetGameSetupData(out GameSetupData game_setup_data)
        {
            if (_game_setup_data.HasValue)
            {
                game_setup_data = _game_setup_data.Value;
                return true;
            }

            game_setup_data = default;
            return false;
        }

        // Clears the stored setup when the scene or owning flow wants to reset the pending match state.
        public void Clear()
        {
            _game_setup_data = null;
        }
    }
}
