namespace Game
{
    public readonly struct GameSetupData
    {
        public GameSetupData(
            PlayerColorChoice player_color_choice,
            DifficultyLevel difficulty_level,
            DiscColor player_disc_color,
            DiscColor ai_disc_color,
            StartingParticipant starting_participant)
        {
            this.player_color_choice = player_color_choice;
            this.difficulty_level = difficulty_level;
            this.player_disc_color = player_disc_color;
            this.ai_disc_color = ai_disc_color;
            this.starting_participant = starting_participant;
        }

        public PlayerColorChoice player_color_choice { get; }

        public DifficultyLevel difficulty_level { get; }

        public DiscColor player_disc_color { get; }

        public DiscColor ai_disc_color { get; }

        public StartingParticipant starting_participant { get; }

        public DiscColor starting_disc_color => DiscColor.Black;
    }
}
