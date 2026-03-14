using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;
using Zenject;

namespace Game
{
    // Bridges the endgame scene buttons to the flow-layer awaiter without owning any scene transitions.
    public class EndGameButtonsView : MonoBehaviour
    {
        private const string BUTTON_MENU_NAME = "ButtonMenu";
        private const string BUTTON_PLAY_AGAIN_NAME = "ButtonPlayAgain";
        private const string BUTTON_QUIT_NAME = "ButtonQuit";
        private const string BACKDROP_DIM_NAME = "BackdropDim";
        private static readonly Color BACKDROP_DIM_COLOR = new Color(0f, 0f, 0f, 0.72f);
        private const float BACKDROP_FADE_IN_DURATION_SECONDS = 0.25f;

        [SerializeField]
        private Button _menu_button;

        [SerializeField]
        private Button _play_again_button;

        [SerializeField]
        private Button _quit_button;

        [SerializeField]
        private EndGameResultView _end_game_result_view;

        private IEndGameChoiceAwaiter _end_game_choice_awaiter;
        private IPlatformSelectionResolver _platform_selection_resolver;
        private EndGameQuitStrategySelector _end_game_quit_strategy_selector;
        private Image _backdrop_dim_image;
        private bool _has_submitted_choice;

        [Inject]
        private void Construct(
            IEndGameChoiceAwaiter end_game_choice_awaiter,
            IPlatformSelectionResolver platform_selection_resolver)
        {
            _end_game_choice_awaiter = end_game_choice_awaiter;
            _platform_selection_resolver = platform_selection_resolver;
        }

        private void Awake()
        {
            EnsureBackdropDim();
            ResolveButtons();
            ResolveResultView();
            CreateQuitStrategySelector();
            RegisterButtonListeners();
        }

        private void Start()
        {
            FadeInBackdropDim();
        }

        private void OnDestroy()
        {
            UnregisterButtonListeners();

            if (_backdrop_dim_image != null)
            {
                _backdrop_dim_image.DOKill();
            }
        }

        private void HandleMenuClicked()
        {
            SubmitChoice(EndGameChoice.Menu);
        }

        private void HandlePlayAgainClicked()
        {
            SubmitChoice(EndGameChoice.PlayAgain);
        }

        private async void HandleQuitClicked()
        {
            await HandleQuitClickedAsync();
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

        private async Task HandleQuitClickedAsync()
        {
            if (_has_submitted_choice)
            {
                return;
            }

            _has_submitted_choice = true;
            SetButtonsInteractable(false);

            try
            {
                IEndGameQuitStrategy quit_strategy = _end_game_quit_strategy_selector.Resolve();
                await quit_strategy.ExecuteAsync();

                if (quit_strategy.keeps_end_game_open)
                {
                    _has_submitted_choice = false;
                    SetButtonsInteractable(true);
                    return;
                }

                _end_game_choice_awaiter.NotifyChoiceRequested(EndGameChoice.Quit);
            }
            catch
            {
                _has_submitted_choice = false;
                SetButtonsInteractable(true);
                throw;
            }
        }

        private void ResolveButtons()
        {
            _menu_button = ResolveButton(_menu_button, BUTTON_MENU_NAME);
            _play_again_button = ResolveButton(_play_again_button, BUTTON_PLAY_AGAIN_NAME);
            _quit_button = ResolveButton(_quit_button, BUTTON_QUIT_NAME);
        }

        private void ResolveResultView()
        {
            if (_end_game_result_view != null)
            {
                return;
            }

            _end_game_result_view = FindFirstObjectByType<EndGameResultView>();
        }

        private void CreateQuitStrategySelector()
        {
            List<IEndGameQuitStrategy> quit_strategies = new List<IEndGameQuitStrategy>
            {
                new EditorEndGameQuitStrategy(),
                new PcEndGameQuitStrategy(),
                new WebGlEndGameQuitStrategy(_end_game_result_view),
                new SecondlifeEndGameQuitStrategy(_end_game_result_view)
            };

            _end_game_quit_strategy_selector =
                new EndGameQuitStrategySelector(quit_strategies, _platform_selection_resolver);
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

        private void EnsureBackdropDim()
        {
            Transform existing_backdrop = transform.Find(BACKDROP_DIM_NAME);
            GameObject backdrop_object = existing_backdrop != null
                ? existing_backdrop.gameObject
                : new GameObject(BACKDROP_DIM_NAME, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            backdrop_object.transform.SetParent(transform, false);
            backdrop_object.transform.SetSiblingIndex(0);

            RectTransform backdrop_transform = backdrop_object.GetComponent<RectTransform>();
            backdrop_transform.anchorMin = Vector2.zero;
            backdrop_transform.anchorMax = Vector2.one;
            backdrop_transform.offsetMin = Vector2.zero;
            backdrop_transform.offsetMax = Vector2.zero;
            backdrop_transform.localScale = Vector3.one;

            _backdrop_dim_image = backdrop_object.GetComponent<Image>();
            _backdrop_dim_image.raycastTarget = false;
            _backdrop_dim_image.color = new Color(
                BACKDROP_DIM_COLOR.r,
                BACKDROP_DIM_COLOR.g,
                BACKDROP_DIM_COLOR.b,
                0f);
        }

        private void FadeInBackdropDim()
        {
            if (_backdrop_dim_image == null)
            {
                return;
            }

            _backdrop_dim_image.DOKill();
            DOTween
                .To(
                    () => _backdrop_dim_image.color,
                    value => _backdrop_dim_image.color = value,
                    new Color(
                        BACKDROP_DIM_COLOR.r,
                        BACKDROP_DIM_COLOR.g,
                        BACKDROP_DIM_COLOR.b,
                        BACKDROP_DIM_COLOR.a),
                    BACKDROP_FADE_IN_DURATION_SECONDS)
                .SetEase(Ease.OutQuad);
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
