using System.Threading.Tasks;

namespace Game
{
    // Holds the currently active gameplay scene controller so the root flow can start or restart matches in place.
    public class GameplaySceneRuntime : IGameplaySceneRuntime
    {
        private readonly object _sync_root = new object();
        private IGameplaySceneController _gameplay_scene_controller;
        private TaskCompletionSource<IGameplaySceneController> _controller_source = CreateControllerSource();

        public void RegisterController(IGameplaySceneController gameplay_scene_controller)
        {
            if (gameplay_scene_controller == null)
            {
                return;
            }

            lock (_sync_root)
            {
                _gameplay_scene_controller = gameplay_scene_controller;
                _controller_source.TrySetResult(gameplay_scene_controller);
            }
        }

        public void UnregisterController(IGameplaySceneController gameplay_scene_controller)
        {
            lock (_sync_root)
            {
                if (_gameplay_scene_controller != gameplay_scene_controller)
                {
                    return;
                }

                _gameplay_scene_controller = null;
                _controller_source = CreateControllerSource();
            }
        }

        public async Task BeginMatchAsync()
        {
            IGameplaySceneController gameplay_scene_controller;

            lock (_sync_root)
            {
                gameplay_scene_controller = _gameplay_scene_controller;
            }

            if (gameplay_scene_controller == null)
            {
                gameplay_scene_controller = await WaitForControllerAsync();
            }

            gameplay_scene_controller.BeginMatch();
        }

        private Task<IGameplaySceneController> WaitForControllerAsync()
        {
            lock (_sync_root)
            {
                return _controller_source.Task;
            }
        }

        private static TaskCompletionSource<IGameplaySceneController> CreateControllerSource()
        {
            return new TaskCompletionSource<IGameplaySceneController>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
