using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game
{
    // This MonoBehaviour creates and manages the 8x8 grid of cell views that render the board.
    // It owns the visual refresh cycle but delegates all game logic to domain services.
    // This keeps the board presentation separate from state and rules while using a GridLayoutGroup for layout.
    public class BoardView : MonoBehaviour
    {
        private const float CELL_SPACING = 2f;
        private const float MINIMUM_CELL_SIZE = 40f;

        [SerializeField]
        private RectTransform _board_root;

        [SerializeField]
        private Texture2D _piece_texture;

        [SerializeField]
        private Texture2D _help_piece_texture;

        [SerializeField]
        private Text _white_count_text;

        [SerializeField]
        private Text _black_count_text;

        [SerializeField]
        private Text _turn_text;

        [SerializeField]
        private Text _message_text;

        private CellView[,] _cell_views;
        private MatchFlowController _match_flow_controller;
        private ReversiBoard _reversi_board;
        private ReversiBoardEvaluator _reversi_board_evaluator;
        private IGameSetupSession _game_setup_session;
        private Sprite _piece_sprite;
        private Sprite _help_piece_sprite;
        private GameSetupData _game_setup_data;
        private bool _has_game_setup_data;

        [Inject]
        private void Construct(
            MatchFlowController match_flow_controller,
            ReversiBoard reversi_board,
            ReversiBoardEvaluator reversi_board_evaluator,
            IGameSetupSession game_setup_session)
        {
            _match_flow_controller = match_flow_controller;
            _reversi_board = reversi_board;
            _reversi_board_evaluator = reversi_board_evaluator;
            _game_setup_session = game_setup_session;
        }

        private void Start()
        {
            _piece_sprite = CreatePieceSprite();
            _help_piece_sprite = CreateHelpPieceSprite();
            _has_game_setup_data = _game_setup_session.TryGetGameSetupData(out _game_setup_data);
            DisableBoardRootRaycastBlockers();
            CreateCellGrid();
            SubscribeToMatchEvents();
            RefreshBoard();
        }

        // Disables raycast interception on any existing Graphic on the board root itself.
        // Without this, a leftover RawImage or Image on the parent would swallow clicks meant for child cells.
        private void DisableBoardRootRaycastBlockers()
        {
            Graphic[] root_graphics = _board_root.GetComponents<Graphic>();

            foreach (Graphic root_graphic in root_graphics)
            {
                root_graphic.raycastTarget = false;
            }
        }

        // Builds the 8x8 grid of cell views inside the board root using a GridLayoutGroup.
        private void CreateCellGrid()
        {
            _cell_views = new CellView[ReversiBoard.BOARD_SIZE, ReversiBoard.BOARD_SIZE];

            SetupGridLayout();

            for (int row = 0; row < ReversiBoard.BOARD_SIZE; row++)
            {
                for (int column = 0; column < ReversiBoard.BOARD_SIZE; column++)
                {
                    CellView cell_view = CreateCellView(row, column);
                    _cell_views[row, column] = cell_view;
                }
            }
        }

        // Configures the GridLayoutGroup on the board root to evenly size cells within the available space.
        // Falls back to a minimum cell size if the layout has not been calculated yet.
        private void SetupGridLayout()
        {
            GridLayoutGroup grid_layout = _board_root.gameObject.GetComponent<GridLayoutGroup>();

            if (grid_layout == null)
            {
                grid_layout = _board_root.gameObject.AddComponent<GridLayoutGroup>();
            }

            Canvas.ForceUpdateCanvases();

            Rect board_rect = _board_root.rect;
            float available_width = board_rect.width - (CELL_SPACING * (ReversiBoard.BOARD_SIZE - 1));
            float available_height = board_rect.height - (CELL_SPACING * (ReversiBoard.BOARD_SIZE - 1));
            float cell_size = Mathf.Min(available_width / ReversiBoard.BOARD_SIZE, available_height / ReversiBoard.BOARD_SIZE);
            cell_size = Mathf.Max(cell_size, MINIMUM_CELL_SIZE);

            grid_layout.cellSize = new Vector2(cell_size, cell_size);
            grid_layout.spacing = new Vector2(CELL_SPACING, CELL_SPACING);
            grid_layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid_layout.constraintCount = ReversiBoard.BOARD_SIZE;
            grid_layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid_layout.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid_layout.childAlignment = TextAnchor.MiddleCenter;
        }

        // Creates a single cell view as a child of the board root.
        // Explicitly assigns the Image as the Button's target graphic so programmatic setup matches editor behavior.
        private CellView CreateCellView(int row, int column)
        {
            GameObject cell_object = new GameObject($"Cell_{row}_{column}");
            cell_object.transform.SetParent(_board_root, false);

            Image cell_image = cell_object.AddComponent<Image>();
            cell_image.raycastTarget = true;

            Button cell_button = cell_object.AddComponent<Button>();
            cell_button.targetGraphic = cell_image;

            Navigation button_navigation = new Navigation();
            button_navigation.mode = Navigation.Mode.None;
            cell_button.navigation = button_navigation;

            Image hint_image = CreateHintImage(cell_object.transform);
            Image disc_image = CreateDiscImage(cell_object.transform);

            CellView cell_view = cell_object.AddComponent<CellView>();
            cell_view.Initialize(row, column, hint_image, disc_image);
            cell_view.on_cell_clicked += HandleCellClicked;

            return cell_view;
        }

        // Creates the legal-move hint overlay that is shown only on the human player's turn.
        private Image CreateHintImage(Transform parent_transform)
        {
            GameObject hint_object = new GameObject("Hint");
            hint_object.transform.SetParent(parent_transform, false);

            RectTransform hint_rect_transform = hint_object.AddComponent<RectTransform>();
            hint_rect_transform.anchorMin = new Vector2(0.22f, 0.22f);
            hint_rect_transform.anchorMax = new Vector2(0.78f, 0.78f);
            hint_rect_transform.offsetMin = Vector2.zero;
            hint_rect_transform.offsetMax = Vector2.zero;

            Image hint_image = hint_object.AddComponent<Image>();
            hint_image.sprite = _help_piece_sprite;
            hint_image.preserveAspect = true;
            hint_image.raycastTarget = false;
            hint_image.enabled = false;
            return hint_image;
        }

        // Creates the disc overlay that sits on top of the green cell background.
        private Image CreateDiscImage(Transform parent_transform)
        {
            GameObject disc_object = new GameObject("Disc");
            disc_object.transform.SetParent(parent_transform, false);

            RectTransform disc_rect_transform = disc_object.AddComponent<RectTransform>();
            disc_rect_transform.anchorMin = new Vector2(0.15f, 0.15f);
            disc_rect_transform.anchorMax = new Vector2(0.85f, 0.85f);
            disc_rect_transform.offsetMin = Vector2.zero;
            disc_rect_transform.offsetMax = Vector2.zero;

            Image disc_image = disc_object.AddComponent<Image>();
            disc_image.sprite = _piece_sprite;
            disc_image.preserveAspect = true;
            disc_image.raycastTarget = false;
            disc_image.enabled = false;
            return disc_image;
        }

        private Sprite CreatePieceSprite()
        {
            if (_piece_texture == null)
            {
                return null;
            }

            Rect sprite_rect = new Rect(0, 0, _piece_texture.width, _piece_texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            return Sprite.Create(_piece_texture, sprite_rect, pivot, 100f);
        }

        private Sprite CreateHelpPieceSprite()
        {
            if (_help_piece_texture == null)
            {
                return null;
            }

            Rect sprite_rect = new Rect(0, 0, _help_piece_texture.width, _help_piece_texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            return Sprite.Create(_help_piece_texture, sprite_rect, pivot, 100f);
        }

        // Listens for match flow events that require a visual refresh.
        private void SubscribeToMatchEvents()
        {
            _match_flow_controller.on_board_changed += RefreshBoard;
            _match_flow_controller.on_turn_changed += HandleTurnChanged;
        }

        // Updates every cell view to reflect the current board state and highlights legal moves.
        private void RefreshBoard()
        {
            for (int row = 0; row < ReversiBoard.BOARD_SIZE; row++)
            {
                for (int column = 0; column < ReversiBoard.BOARD_SIZE; column++)
                {
                    CellState cell_state = _reversi_board.GetCellState(row, column);
                    _cell_views[row, column].SetCellState(cell_state);
                }
            }

            HighlightLegalMoves();
            RefreshHud();
        }

        // Shows hint markers on cells where the human player can legally place a disc.
        private void HighlightLegalMoves()
        {
            List<BoardPosition> legal_moves = _match_flow_controller.GetCurrentLegalMoves();

            foreach (BoardPosition move in legal_moves)
            {
                _cell_views[move.row, move.column].ShowLegalMoveHint();
            }
        }

        // Forwards a cell click to the match flow controller as a human move submission.
        private void HandleCellClicked(int row, int column)
        {
            BoardPosition position = new BoardPosition(row, column);
            _match_flow_controller.SubmitHumanMove(position);
        }

        private void HandleTurnChanged(TurnParticipant current_turn_participant)
        {
            RefreshHud();
        }

        private void RefreshHud()
        {
            if (_white_count_text == null || _black_count_text == null || _turn_text == null || _message_text == null)
            {
                return;
            }

            int white_count = _reversi_board_evaluator.CountDiscs(_reversi_board, CellState.White);
            int black_count = _reversi_board_evaluator.CountDiscs(_reversi_board, CellState.Black);

            _white_count_text.text = $"{GetOwnerLabel(CellState.White)} White: {white_count}";
            _black_count_text.text = $"{GetOwnerLabel(CellState.Black)} Black: {black_count}";

            if (_match_flow_controller.match_phase == MatchPhase.WaitingToStart)
            {
                _turn_text.text = "Turn: Starting...";
                _message_text.text = "Loading match...";
                return;
            }

            TurnParticipant current_turn_participant = _match_flow_controller.current_turn_participant;
            CellState current_turn_color = _match_flow_controller.GetCurrentTurnDiscColor();
            _turn_text.text = $"Turn: {GetParticipantLabel(current_turn_participant)} ({GetDiscColorLabel(current_turn_color)})";
            _message_text.text = current_turn_participant == TurnParticipant.Human
                ? string.Empty
                : "AI thinking...wait";
        }

        private string GetOwnerLabel(CellState disc_color)
        {
            if (_has_game_setup_data == false)
            {
                return disc_color == CellState.Black ? "YOU" : "AI";
            }

            if (_game_setup_data.ai_disc_color == DiscColor.White)
            {
                return disc_color == CellState.White ? "AI" : "YOU";
            }

            return disc_color == CellState.White ? "YOU" : "AI";
        }

        private static string GetParticipantLabel(TurnParticipant turn_participant)
        {
            return turn_participant == TurnParticipant.Human ? "YOU" : "AI";
        }

        private static string GetDiscColorLabel(CellState disc_color)
        {
            return disc_color == CellState.White ? "White" : "Black";
        }

        private void OnDestroy()
        {
            if (_match_flow_controller != null)
            {
                _match_flow_controller.on_board_changed -= RefreshBoard;
                _match_flow_controller.on_turn_changed -= HandleTurnChanged;
            }

            if (_cell_views != null)
            {
                for (int row = 0; row < ReversiBoard.BOARD_SIZE; row++)
                {
                    for (int column = 0; column < ReversiBoard.BOARD_SIZE; column++)
                    {
                        if (_cell_views[row, column] != null)
                        {
                            _cell_views[row, column].on_cell_clicked -= HandleCellClicked;
                        }
                    }
                }
            }
        }
    }
}
