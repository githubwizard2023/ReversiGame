namespace Game
{
    // This enum represents the final match result from the player's perspective.
    // It avoids booleans like 'didPlayerWin' and captures the draw case explicitly.
    // This keeps game-over evaluation readable and unambiguous.
    public enum MatchOutcome
    {
        HumanWin,
        AIWin,
        Draw
    }
}
