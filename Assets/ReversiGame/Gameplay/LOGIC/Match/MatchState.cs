namespace Game
{
    // This class holds the mutable runtime state of an active Reversi match.
    // It keeps turn tracking, pass state, and match phase in one place instead of scattering flags.
    // This makes the current match situation queryable without coupling to flow control logic.
    public class MatchState
    {
        // Tracks which participant's turn it currently is.
        public TurnParticipant current_turn_participant { get; private set; }

        // True when the previous turn resulted in a pass because no legal moves were available.
        public bool did_previous_turn_pass { get; private set; }

        // Tracks the high-level phase of the match for flow control decisions.
        public MatchPhase match_phase { get; private set; }

        // Stores the final match result once the match reaches the Finished phase.
        public MatchOutcome match_outcome { get; private set; }

        // Initializes the match state for a new game with the given starting participant.
        public void Initialize(TurnParticipant starting_participant)
        {
            current_turn_participant = starting_participant;
            did_previous_turn_pass = false;
            match_phase = MatchPhase.InProgress;
        }

        // Records that the current turn resulted in a pass and advances the turn.
        public void RecordPass()
        {
            did_previous_turn_pass = true;
            AdvanceTurn();
        }

        // Records a successful move and resets the pass flag before advancing the turn.
        public void RecordMove()
        {
            did_previous_turn_pass = false;
            AdvanceTurn();
        }

        // Marks the match as finished with the determined outcome.
        public void Finish(MatchOutcome outcome)
        {
            match_outcome = outcome;
            match_phase = MatchPhase.Finished;
        }

        // Switches the active turn to the other participant.
        private void AdvanceTurn()
        {
            if (current_turn_participant == TurnParticipant.Human)
            {
                current_turn_participant = TurnParticipant.AI;
                return;
            }

            current_turn_participant = TurnParticipant.Human;
        }
    }
}
