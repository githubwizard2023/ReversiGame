namespace Game
{
    public readonly struct GameSetupSelection
    {
        public GameSetupSelection(
            PlayerColorChoice player_color_choice,
            DifficultyLevel difficulty_level)
        {
            this.player_color_choice = player_color_choice;
            this.difficulty_level = difficulty_level;
        }

        public PlayerColorChoice player_color_choice { get; }

        public DifficultyLevel difficulty_level { get; }
    }
}
