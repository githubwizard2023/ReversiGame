namespace Game
{
    // This enum defines the major phases in the game's high-level flow.
    // It acts as the state key for the table-driven manager strategy lookup.
    // This keeps flow transitions explicit and readable across the project.
    public enum GameState
    {
        Init,
        Splash,
        MainMenu,
        Settings,
        Instructions,
        Gameplay,
        EndGame,
        Quit
    }
}
