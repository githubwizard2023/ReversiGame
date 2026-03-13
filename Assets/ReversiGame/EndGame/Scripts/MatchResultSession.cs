namespace Game
{
    // Stores the last completed match result so the dedicated endgame scene can render it after gameplay unloads.
    public class MatchResultSession : IMatchResultSession
    {
        private MatchOutcome? _match_outcome;

        public void Store(MatchOutcome match_outcome)
        {
            _match_outcome = match_outcome;
        }

        public bool TryGetMatchOutcome(out MatchOutcome match_outcome)
        {
            if (_match_outcome.HasValue)
            {
                match_outcome = _match_outcome.Value;
                return true;
            }

            match_outcome = default;
            return false;
        }

        public void Clear()
        {
            _match_outcome = null;
        }
    }
}
