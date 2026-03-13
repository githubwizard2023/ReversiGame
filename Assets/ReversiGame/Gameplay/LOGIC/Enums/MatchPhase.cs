namespace Game
{
    // This enum represents the high-level phase of an active Reversi match.
    // It replaces scattered boolean flags with a single readable state.
    // This keeps match flow decisions clear in the controller without nested conditionals.
    public enum MatchPhase
    {
        WaitingToStart,
        InProgress,
        Finished
    }
}
