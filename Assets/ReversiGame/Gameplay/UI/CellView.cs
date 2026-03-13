using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    // This MonoBehaviour represents a single cell on the Reversi board grid.
    // It renders the disc state through its Image component and forwards click input upward.
    // This keeps each cell self-contained so the board view can manage all 64 cells uniformly.
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class CellView : MonoBehaviour
    {
        private static readonly Color EMPTY_CELL_COLOR = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        private static readonly Color BLACK_DISC_COLOR = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        private static readonly Color WHITE_DISC_COLOR = new Color(0.95f, 0.95f, 0.95f, 1.0f);

        private Image _cell_background_image;
        private Image _hint_image;
        private Image _disc_image;
        private Button _cell_button;
        private int _row;
        private int _column;

        // Raised when the player clicks this cell, carrying the cell's board coordinates.
        public event Action<int, int> on_cell_clicked;

        // Initializes the cell with its board coordinates and wires the click listener.
        public void Initialize(int row, int column, Image hint_image, Image disc_image)
        {
            _row = row;
            _column = column;

            _cell_background_image = GetComponent<Image>();
            _hint_image = hint_image;
            _disc_image = disc_image;
            _cell_button = GetComponent<Button>();

            _cell_button.onClick.AddListener(HandleCellClicked);

            SetEmpty();
        }

        // Updates the visual appearance of this cell to match the given board state.
        public void SetCellState(CellState cell_state)
        {
            switch (cell_state)
            {
                case CellState.Black:
                    SetDiscColor(BLACK_DISC_COLOR);
                    break;

                case CellState.White:
                    SetDiscColor(WHITE_DISC_COLOR);
                    break;

                default:
                    SetEmpty();
                    break;
            }
        }

        // Renders a translucent hint marker to indicate this cell is a legal move target.
        public void ShowLegalMoveHint()
        {
            _cell_background_image.color = EMPTY_CELL_COLOR;
            _hint_image.enabled = true;
            _disc_image.enabled = false;
        }

        // Renders the cell as an empty green square with no disc.
        private void SetEmpty()
        {
            _cell_background_image.color = EMPTY_CELL_COLOR;
            _hint_image.enabled = false;
            _disc_image.enabled = false;
        }

        // Renders a disc on this cell with the given color.
        private void SetDiscColor(Color disc_color)
        {
            _cell_background_image.color = EMPTY_CELL_COLOR;
            _hint_image.enabled = false;
            _disc_image.enabled = true;
            _disc_image.color = disc_color;
        }

        // Forwards the button click to any listener with this cell's coordinates.
        private void HandleCellClicked()
        {
            on_cell_clicked?.Invoke(_row, _column);
        }

        private void OnDestroy()
        {
            if (_cell_button != null)
            {
                _cell_button.onClick.RemoveListener(HandleCellClicked);
            }
        }
    }
}
