namespace Game
{
    // This static holder centralizes scene identifiers used by the game flow.
    // It avoids duplicated string literals across strategies and services.
    // This keeps scene transition references explicit and easy to update.
    public static class GameSceneNames
    {
        public const string SPLASH_SCENE = "SplashScene";
        public const string INSTRUCTIONS_SCENE = "InstructionsScene";
        public const string GAME_SETTINGS_SCENE = "GameSettingsScene";
        public const string GAMEPLAY_SCENE = "GameplayScene";
        public const string END_GAME_SCENE = "EndGameScene";
    }
}
