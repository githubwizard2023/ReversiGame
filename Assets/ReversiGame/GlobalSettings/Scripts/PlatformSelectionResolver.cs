using UnityEngine;

namespace Game
{
    // This service translates the configured platform setting into the effective runtime platform.
    // It follows a resolver role so AUTO can stay centralized instead of duplicated across callers.
    // This fits the project because the platform rule is shared by quit flow and endgame presentation.
    public class PlatformSelectionResolver : IPlatformSelectionResolver
    {
        private readonly GlobalGameSettings _global_game_settings;

        public PlatformSelectionResolver(GlobalGameSettings global_game_settings)
        {
            _global_game_settings = global_game_settings;
        }

        public Platforms ResolvePlatform()
        {
            if (_global_game_settings.platform != Platforms.AUTO)
            {
                return _global_game_settings.platform;
            }

            return ResolveAutomaticPlatform();
        }

        private static Platforms ResolveAutomaticPlatform()
        {
#if UNITY_EDITOR
            return Platforms.EDITOR;
#else
            switch (Application.platform)
            {
                case RuntimePlatform.WebGLPlayer:
                    return Platforms.WebGL;

                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.LinuxPlayer:
                    return Platforms.PC;

                default:
                    return Platforms.PC;
            }
#endif
        }
    }
}
