using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Game
{
    // This class connects the global mouse click input to shared UI click audio playback.
    // It follows a root-level input listener pattern so the click sound stays centralized instead of duplicated across views.
    // This fits the current game because the click sound should work across scenes through one persistent Zenject binding.
    public class MouseClickAudioTrigger : IInitializable, IDisposable
    {
        private const string LEFT_MOUSE_BUTTON_BINDING = "<Mouse>/leftButton";

        private readonly IAudioService _audio_service;
        private readonly AudioClip _button_click_clip;
        private readonly InputAction _left_mouse_click_action;

        public MouseClickAudioTrigger(
            IAudioService audio_service,
            [Inject(Id = AudioClipIds.BUTTON_CLICK)] AudioClip button_click_clip)
        {
            _audio_service = audio_service;
            _button_click_clip = button_click_clip;
            _left_mouse_click_action = new InputAction(
                name: "GlobalLeftMouseClick",
                type: InputActionType.Button,
                binding: LEFT_MOUSE_BUTTON_BINDING);
        }

        // Starts listening once the root container is ready so every future left-click can reuse the shared audio service.
        public void Initialize()
        {
            _left_mouse_click_action.performed += HandleLeftMouseClickPerformed;
            _left_mouse_click_action.Enable();
        }

        // Unhooks and disposes the action so the persistent root container leaves no dangling input subscriptions.
        public void Dispose()
        {
            _left_mouse_click_action.performed -= HandleLeftMouseClickPerformed;
            _left_mouse_click_action.Disable();
            _left_mouse_click_action.Dispose();
        }

        // Plays the configured click clip only when the shared clip reference exists.
        private void HandleLeftMouseClickPerformed(InputAction.CallbackContext callback_context)
        {
            if (_button_click_clip == null)
            {
                return;
            }

            _audio_service.Play(_button_click_clip);
        }
    }
}
