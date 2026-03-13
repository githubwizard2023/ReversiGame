using System;

namespace Game
{
    // This class resolves UI setup choices into concrete match settings for Reversi.
    // It acts as a small domain service that keeps randomization and start-order rules out of the view.
    // This fits the project by keeping setup logic pure and easy to reuse from later flow code.
    public class GameSetupResolver : IGameSetupResolver
    {
        private readonly Random _random;

        public GameSetupResolver()
        {
            _random = new Random();
        }

        // Resolves the player's final disc color and derives who starts from the Reversi black-first rule.
        public GameSetupData Resolve(GameSetupSelection game_setup_selection)
        {
            DiscColor player_disc_color = ResolvePlayerDiscColor(game_setup_selection.player_color_choice);
            DiscColor ai_disc_color = ResolveOpponentDiscColor(player_disc_color);
            StartingParticipant starting_participant = ResolveStartingParticipant(player_disc_color);

            GameSetupData game_setup_data = new GameSetupData(
                game_setup_selection.player_color_choice,
                game_setup_selection.difficulty_level,
                player_disc_color,
                ai_disc_color,
                starting_participant);

            return game_setup_data;
        }

        // Random selection is resolved only when the match is about to start so the stored setup is final.
        private DiscColor ResolvePlayerDiscColor(PlayerColorChoice player_color_choice)
        {
            switch (player_color_choice)
            {
                case PlayerColorChoice.Black:
                    return DiscColor.Black;

                case PlayerColorChoice.White:
                    return DiscColor.White;

                case PlayerColorChoice.Random:
                    return _random.Next(0, 2) == 0
                        ? DiscColor.Black
                        : DiscColor.White;

                default:
                    throw new ArgumentOutOfRangeException(nameof(player_color_choice), player_color_choice, null);
            }
        }

        // Reversi is a two-color game, so the AI always receives the opposite disc color.
        private static DiscColor ResolveOpponentDiscColor(DiscColor player_disc_color)
        {
            if (player_disc_color == DiscColor.Black)
            {
                return DiscColor.White;
            }

            return DiscColor.Black;
        }

        // Black always starts first in classic Reversi, so the starter is derived from the resolved player color.
        private static StartingParticipant ResolveStartingParticipant(DiscColor player_disc_color)
        {
            if (player_disc_color == DiscColor.Black)
            {
                return StartingParticipant.Human;
            }

            return StartingParticipant.AI;
        }
    }
}
