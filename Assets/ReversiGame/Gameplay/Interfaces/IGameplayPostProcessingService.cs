using UnityEngine;

namespace Game
{
    public interface IGameplayPostProcessingService
    {
        void ConfigureGameplayCanvas(Canvas gameplay_canvas);

        void SetEndGameBlurEnabled(bool is_enabled);
    }
}
