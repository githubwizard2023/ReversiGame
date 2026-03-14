using UnityEngine;
using UnityEngine.EventSystems;

namespace Game
{
    public class SplashContinueClickHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
    {
        private ISplashContinueAwaiter _splash_continue_awaiter;

        [Zenject.Inject]
        private void Construct(ISplashContinueAwaiter splash_continue_awaiter)
        {
            _splash_continue_awaiter = splash_continue_awaiter;
        }

        public void OnPointerClick(PointerEventData event_data)
        {
            _splash_continue_awaiter.NotifyContinueRequested();
        }

        public void OnPointerDown(PointerEventData event_data)
        {
            _splash_continue_awaiter.NotifyContinueRequested();
        }
    }
}
