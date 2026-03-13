namespace Game
{
    public interface IGameSetupSession
    {
        void Store(GameSetupData game_setup_data);

        bool TryGetGameSetupData(out GameSetupData game_setup_data);

        void Clear();
    }
}
