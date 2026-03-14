using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game
{
    // This class is the thin UI bridge for the dedicated settings scene.
    // It follows the view-controller split so inspector wiring stays here while setup flow stays in plain C# services.
    // This fits the solution because the moved scene should only collect button choices and forward them.
    public class GameSetupScreen : MonoBehaviour
    {
        private const string BUTTON_BLACK_NAME = "ButtonBlack";
        private const string BUTTON_WHITE_NAME = "ButtonWhite";
        private const string BUTTON_RANDOM_NAME = "ButtonRandom";
        private const string BUTTON_EASY_NAME = "ButtonEasy";
        private const string BUTTON_MEDIUM_NAME = "ButtonMedium";
        private const string BUTTON_HARD_NAME = "ButtonHard";
        private const string BUTTON_START_NAME = "ButtonStart";

        private static readonly Color SELECTED_BUTTON_COLOR = new Color(1f, 1f, 1f, 200f / 255f);
        private static readonly Color UNSELECTED_BUTTON_COLOR = new Color(1f, 1f, 1f, 0f);

        private const PlayerColorChoice DEFAULT_PLAYER_COLOR_CHOICE = PlayerColorChoice.Random;
        private const DifficultyLevel DEFAULT_DIFFICULTY_LEVEL = DifficultyLevel.Easy;

        [SerializeField]
        private Button _black_button;

        [SerializeField]
        private Button _white_button;

        [SerializeField]
        private Button _random_button;

        [SerializeField]
        private Button _easy_button;

        [SerializeField]
        private Button _medium_button;

        [SerializeField]
        private Button _hard_button;

        [SerializeField]
        private Button _start_button;

        private IGameSetupScreenController _game_setup_screen_controller;
        private IGameSetupSession _game_setup_session;
        private PlayerColorChoice _selected_player_color_choice = DEFAULT_PLAYER_COLOR_CHOICE;
        private DifficultyLevel _selected_difficulty_level = DEFAULT_DIFFICULTY_LEVEL;
        private bool _is_start_in_progress;

        [Inject]
        private void Construct(
            IGameSetupScreenController game_setup_screen_controller,
            IGameSetupSession game_setup_session)
        {
            _game_setup_screen_controller = game_setup_screen_controller;
            _game_setup_session = game_setup_session;
        }

        // Finds scene buttons when inspector references are missing, then hooks the click listeners once.
        private void Awake()
        {
            ResolveButtons();
            RegisterButtonListeners();
            ApplyStoredSelectionsOrDefaults();
        }

        // Removes listeners so the settings scene can unload without leaving stale callbacks behind.
        private void OnDestroy()
        {
            UnregisterButtonListeners();
        }

        // Builds the current selection snapshot and forwards it to the controller for scene handoff.
        private async void HandleStartButtonClicked()
        {
            if (_is_start_in_progress)
            {
                return;
            }

            if (_start_button == null)
            {
                Debug.LogError("GameSetupScreen requires a Start button.");
                return;
            }

            _is_start_in_progress = true;
            _start_button.interactable = false;

            try
            {
                GameSetupSelection game_setup_selection = BuildGameSetupSelection();
                await _game_setup_screen_controller.StartGameAsync(game_setup_selection);
            }
            finally
            {
                if (this != null)
                {
                    _is_start_in_progress = false;

                    if (_start_button != null)
                    {
                        _start_button.interactable = true;
                    }
                }
            }
        }

        // Stores the selected player color and refreshes the visual state of the color buttons.
        private void HandleBlackButtonClicked()
        {
            SetPlayerColorChoice(PlayerColorChoice.Black);
        }

        // Stores the selected player color and refreshes the visual state of the color buttons.
        private void HandleWhiteButtonClicked()
        {
            SetPlayerColorChoice(PlayerColorChoice.White);
        }

        // Stores the selected player color and refreshes the visual state of the color buttons.
        private void HandleRandomButtonClicked()
        {
            SetPlayerColorChoice(PlayerColorChoice.Random);
        }

        // Stores the selected difficulty and refreshes the visual state of the difficulty buttons.
        private void HandleEasyButtonClicked()
        {
            SetDifficultyLevel(DifficultyLevel.Easy);
        }

        // Stores the selected difficulty and refreshes the visual state of the difficulty buttons.
        private void HandleMediumButtonClicked()
        {
            SetDifficultyLevel(DifficultyLevel.Medium);
        }

        // Stores the selected difficulty and refreshes the visual state of the difficulty buttons.
        private void HandleHardButtonClicked()
        {
            SetDifficultyLevel(DifficultyLevel.Hard);
        }

        // Keeps the scene in a valid startup state and shows the requested default choices immediately.
        private void ApplyStoredSelectionsOrDefaults()
        {
            if (_game_setup_session.TryGetGameSetupData(out GameSetupData game_setup_data))
            {
                _selected_player_color_choice = game_setup_data.player_color_choice;
                _selected_difficulty_level = game_setup_data.difficulty_level;
                RefreshSelectionVisuals();
                return;
            }

            _selected_player_color_choice = DEFAULT_PLAYER_COLOR_CHOICE;
            _selected_difficulty_level = DEFAULT_DIFFICULTY_LEVEL;
            RefreshSelectionVisuals();
        }

        // Reads the current UI state into one immutable selection value for the controller layer.
        private GameSetupSelection BuildGameSetupSelection()
        {
            GameSetupSelection game_setup_selection =
                new GameSetupSelection(_selected_player_color_choice, _selected_difficulty_level);

            return game_setup_selection;
        }

        // Uses scene object names as a fallback so the screen still works when the component is added after scene layout.
        private void ResolveButtons()
        {
            _black_button = ResolveButton(_black_button, BUTTON_BLACK_NAME);
            _white_button = ResolveButton(_white_button, BUTTON_WHITE_NAME);
            _random_button = ResolveButton(_random_button, BUTTON_RANDOM_NAME);
            _easy_button = ResolveButton(_easy_button, BUTTON_EASY_NAME);
            _medium_button = ResolveButton(_medium_button, BUTTON_MEDIUM_NAME);
            _hard_button = ResolveButton(_hard_button, BUTTON_HARD_NAME);
            _start_button = ResolveButton(_start_button, BUTTON_START_NAME);
        }

        // Hooks each scene button to a focused method so the UI state stays explicit and easy to inspect.
        private void RegisterButtonListeners()
        {
            AddListener(_black_button, HandleBlackButtonClicked);
            AddListener(_white_button, HandleWhiteButtonClicked);
            AddListener(_random_button, HandleRandomButtonClicked);
            AddListener(_easy_button, HandleEasyButtonClicked);
            AddListener(_medium_button, HandleMediumButtonClicked);
            AddListener(_hard_button, HandleHardButtonClicked);
            AddListener(_start_button, HandleStartButtonClicked);
        }

        // Mirrors RegisterButtonListeners so the lifetime of every callback stays balanced.
        private void UnregisterButtonListeners()
        {
            RemoveListener(_black_button, HandleBlackButtonClicked);
            RemoveListener(_white_button, HandleWhiteButtonClicked);
            RemoveListener(_random_button, HandleRandomButtonClicked);
            RemoveListener(_easy_button, HandleEasyButtonClicked);
            RemoveListener(_medium_button, HandleMediumButtonClicked);
            RemoveListener(_hard_button, HandleHardButtonClicked);
            RemoveListener(_start_button, HandleStartButtonClicked);
        }

        // Centralizes button fallback lookup so each field can prefer inspector wiring without depending on it.
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

        // Keeps listener registration null-safe because scene assembly may still be in progress.
        private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.AddListener(action);
        }

        // Keeps listener removal null-safe for scene unload and partial editor setups.
        private static void RemoveListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
        }

        // Updates the selected color state in one place so UI refresh stays consistent.
        private void SetPlayerColorChoice(PlayerColorChoice player_color_choice)
        {
            _selected_player_color_choice = player_color_choice;
            RefreshSelectionVisuals();
        }

        // Updates the selected difficulty state in one place so UI refresh stays consistent.
        private void SetDifficultyLevel(DifficultyLevel difficulty_level)
        {
            _selected_difficulty_level = difficulty_level;
            RefreshSelectionVisuals();
        }

        // Highlights the active choice buttons so the player can see the pending setup before starting the match.
        private void RefreshSelectionVisuals()
        {
            SetButtonSelected(_black_button, _selected_player_color_choice == PlayerColorChoice.Black);
            SetButtonSelected(_white_button, _selected_player_color_choice == PlayerColorChoice.White);
            SetButtonSelected(_random_button, _selected_player_color_choice == PlayerColorChoice.Random);

            SetButtonSelected(_easy_button, _selected_difficulty_level == DifficultyLevel.Easy);
            SetButtonSelected(_medium_button, _selected_difficulty_level == DifficultyLevel.Medium);
            SetButtonSelected(_hard_button, _selected_difficulty_level == DifficultyLevel.Hard);
        }

        // Uses the button image as a simple selected-state background without requiring extra scene objects.
        private static void SetButtonSelected(Button button, bool is_selected)
        {
            if (button == null || button.image == null)
            {
                return;
            }

            button.image.color = is_selected
                ? SELECTED_BUTTON_COLOR
                : UNSELECTED_BUTTON_COLOR;
        }
    }
}