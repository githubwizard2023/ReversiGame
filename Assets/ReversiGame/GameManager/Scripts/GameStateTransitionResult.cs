namespace Game
{
    // This value object carries the outcome of a single state strategy execution.
    // It keeps transition decisions explicit instead of spreading flow flags across the manager.
    // This fits the table-driven orchestration model used by the game flow.
    public readonly struct GameStateTransitionResult
    {
        public GameStateTransitionResult(GameState? next_game_state, string target_scene_name)
        {
            this.next_game_state = next_game_state;
            this.target_scene_name = target_scene_name;
        }

        public GameState? next_game_state { get; }

        public string target_scene_name { get; }
    }
}
