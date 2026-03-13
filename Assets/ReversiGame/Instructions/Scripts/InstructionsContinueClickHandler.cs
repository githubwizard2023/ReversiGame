using UnityEngine;
using UnityEngine.EventSystems;

namespace Game
{
    // This class is the thin UI bridge that turns an instructions-screen click into a flow continue request.
    // It keeps scene input handling inside the view layer while the state transition remains in the game-flow logic.
    // This matches the project's preference for simple MonoBehaviours with one focused responsibility.
    public class InstructionsContinueClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private IInstructionsContinueAwaiter _instructions_continue_awaiter;

        [Zenject.Inject]
        private void Construct(IInstructionsContinueAwaiter instructions_continue_awaiter)
        {
            _instructions_continue_awaiter = instructions_continue_awaiter;
        }

        // Forwards the click so the instructions state can continue into the setup screen.
        public void OnPointerClick(PointerEventData event_data)
        {
            _instructions_continue_awaiter.NotifyContinueRequested();
        }
    }
}
