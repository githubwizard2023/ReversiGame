namespace Game
{
    // This class defines stable Zenject identifiers for shared audio clip bindings.
    // It follows a simple constants holder pattern so callers can request specific clips without string duplication.
    // This fits the project because global UI sounds should be injectable across scenes with low coupling.
    public static class AudioClipIds
    {
        public const string BUTTON_CLICK = "ButtonClick";
    }
}
