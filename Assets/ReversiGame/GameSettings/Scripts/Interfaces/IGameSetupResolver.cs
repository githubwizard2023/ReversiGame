namespace Game
{
    public interface IGameSetupResolver
    {
        GameSetupData Resolve(GameSetupSelection game_setup_selection);
    }
}
