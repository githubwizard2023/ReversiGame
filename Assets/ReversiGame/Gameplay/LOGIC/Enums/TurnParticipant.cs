namespace Game
{
    // This enum identifies which participant owns the current turn during a match.
    // It mirrors StartingParticipant but is scoped to in-match turn progression.
    // This keeps turn logic explicit and avoids conflating setup-time data with runtime flow.
    public enum TurnParticipant
    {
        Human,
        AI
    }
}
