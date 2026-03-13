namespace Game
{
    public interface IMatchResultSession
    {
        void Store(MatchOutcome match_outcome);

        bool TryGetMatchOutcome(out MatchOutcome match_outcome);

        void Clear();
    }
}
