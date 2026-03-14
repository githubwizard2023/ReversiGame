namespace Game
{
    // This interface exposes one place to resolve the effective platform from settings and runtime state.
    // It acts as a small boundary so UI flow and quit logic can share the same decision source.
    // This fits the project because platform selection is configuration-driven but still runtime-sensitive.
    public interface IPlatformSelectionResolver
    {
        Platforms ResolvePlatform();
    }
}
