using Cinemachine;
using UnityEngine;

namespace Game
{
    // Owns the persistent camera-backed UI rendering setup.
    public class GameplayPostProcessingService : IGameplayPostProcessingService
    {
        private const float GAMEPLAY_CANVAS_PLANE_DISTANCE = 100f;

        private Camera _game_camera;
        private bool _has_initialized;

        public void ConfigureGameplayCanvas(Canvas gameplay_canvas)
        {
            if (gameplay_canvas == null)
            {
                return;
            }

            EnsureInitialized();

            if (_game_camera == null)
            {
                return;
            }

            gameplay_canvas.renderMode = RenderMode.ScreenSpaceCamera;
            gameplay_canvas.worldCamera = _game_camera;
            gameplay_canvas.planeDistance = GAMEPLAY_CANVAS_PLANE_DISTANCE;
        }

        public void SetEndGameBlurEnabled(bool is_enabled)
        {
        }

        private void EnsureInitialized()
        {
            if (_has_initialized)
            {
                return;
            }

            _has_initialized = true;
            _game_camera = ResolveGameCamera();
        }

        private static Camera ResolveGameCamera()
        {
            CinemachineBrain cinemachine_brain = Object.FindFirstObjectByType<CinemachineBrain>();

            if (cinemachine_brain != null)
            {
                return cinemachine_brain.GetComponent<Camera>();
            }

            return Object.FindFirstObjectByType<Camera>();
        }
    }
}
