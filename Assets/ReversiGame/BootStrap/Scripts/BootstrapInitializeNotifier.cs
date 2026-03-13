using UnityEngine;
using Zenject;

namespace Game
{
    // This bootstrap component is the explicit bridge between the startup scene and the game flow.
    // It keeps scene-driven initialization outside the manager while still letting the manager own flow decisions.
    // This makes startup intent visible in the bootstrap scene instead of hiding it in container initialization.
    public class BootstrapInitializeNotifier : MonoBehaviour
    {
        private IGameManager _game_manager;

        [Inject]
        private void Construct(IGameManager game_manager)
        {
            _game_manager = game_manager;
        }

        // Notifies the manager once the bootstrap scene is active and ready to hand off control.
        // This is the current trigger point for leaving bootstrap and entering the game flow.
        private void Start()
        {
            _game_manager.NotifyBootstrapInitializeCompleted();
        }
    }
}
