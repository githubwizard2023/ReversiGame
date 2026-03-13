using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game
{
    // Bridges the endgame scene buttons to the flow-layer awaiter without owning any scene transitions.
    public class EndGameButtonsView : MonoBehaviour
    {
        private const string BUTTON_MENU_NAME = "ButtonMenu";
        private const string BUTTON_PLAY_AGAIN_NAME = "ButtonPlayAgain";
        private const string BUTTON_QUIT_NAME = "ButtonQuit";

        [SerializeField]
        private Button _menu_button;

        [SerializeField]
        private Button _play_again_button;

        [SerializeField]
        private Button _quit_button;

        private IEndGameChoiceAwaiter _end_game_choice_awaiter;
        private bool _has_submitted_choice;

        [Inject]
        private void Construct(IEndGameChoiceAwaiter end_game_choice_awaiter)
        {
            _end_game_choice_awaiter = end_game_choice_awaiter;
        }

        private void Awake()
        {
            ResolveButtons();
            RegisterButtonListeners();
        }

        private void OnDestroy()
        {
            UnregisterButtonListeners();
        }

        private void HandleMenuClicked()
        {
            SubmitChoice(EndGameChoice.Menu);
        }

        private void HandlePlayAgainClicked()
        {
            SubmitChoice(EndGameChoice.PlayAgain);
        }

        private void HandleQuitClicked()
        {
            SubmitChoice(EndGameChoice.Quit);
        }

        private void SubmitChoice(EndGameChoice end_game_choice)
        {
            if (_has_submitted_choice)
            {
                return;
            }

            _has_submitted_choice = true;
            SetButtonsInteractable(false);
            _end_game_choice_awaiter.NotifyChoiceRequested(end_game_choice);
        }

        private void ResolveButtons()
        {
            _menu_button = ResolveButton(_menu_button, BUTTON_MENU_NAME);
            _play_again_button = ResolveButton(_play_again_button, BUTTON_PLAY_AGAIN_NAME);
            _quit_button = ResolveButton(_quit_button, BUTTON_QUIT_NAME);
        }

        private void RegisterButtonListeners()
        {
            AddListener(_menu_button, HandleMenuClicked);
            AddListener(_play_again_button, HandlePlayAgainClicked);
            AddListener(_quit_button, HandleQuitClicked);
        }

        private void UnregisterButtonListeners()
        {
            RemoveListener(_menu_button, HandleMenuClicked);
            RemoveListener(_play_again_button, HandlePlayAgainClicked);
            RemoveListener(_quit_button, HandleQuitClicked);
        }

        private void SetButtonsInteractable(bool is_interactable)
        {
            SetButtonInteractable(_menu_button, is_interactable);
            SetButtonInteractable(_play_again_button, is_interactable);
            SetButtonInteractable(_quit_button, is_interactable);
        }

        private static Button ResolveButton(Button configured_button, string button_name)
        {
            if (configured_button != null)
            {
                return configured_button;
            }

            GameObject button_game_object = GameObject.Find(button_name);

            if (button_game_object == null)
            {
                return null;
            }

            return button_game_object.GetComponent<Button>();
        }

        private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.AddListener(action);
        }

        private static void RemoveListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
        }

        private static void SetButtonInteractable(Button button, bool is_interactable)
        {
            if (button == null)
            {
                return;
            }

            button.interactable = is_interactable;
        }
    }
}
