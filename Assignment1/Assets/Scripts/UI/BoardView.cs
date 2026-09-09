// Author: Timothy Hand
// Email: thand556@gmail.com
// Made with AI

using UnityEngine;
using UnityEngine.UI;
using SlidingPuzzle.Core;

namespace SlidingPuzzle.UI
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private RectTransform gridContainer;
        [SerializeField] private TileView tilePrefab;
        [SerializeField] private float totalGridSize = 300f;
        [SerializeField] private float slideDuration = 0.15f;

        private TileView[] tileViewsByValue;
        private float cellSize;

        private void Awake()
        {
            // A GridLayoutGroup would re-drive anchoredPosition every layout pass,
            // fighting the manual slide animation below.
            GridLayoutGroup layoutGroup = gridContainer.GetComponent<GridLayoutGroup>();
            if (layoutGroup != null)
            {
                Destroy(layoutGroup);
            }
        }

        public void BuildGrid(int size, System.Action<Vector2Int> onTileSelected)
        {
            foreach (Transform child in gridContainer)
            {
                Destroy(child.gameObject);
            }

            cellSize = totalGridSize / size;

            tileViewsByValue = new TileView[size * size];
            for (int i = 0; i < tileViewsByValue.Length; i++)
            {
                TileView view = Instantiate(tilePrefab, gridContainer);
                view.Init(onTileSelected);
                view.SetVisualSize(cellSize * 0.88f);
                tileViewsByValue[i] = view;
            }
        }

        public void Refresh(Board board, bool animate = true)
        {
            for (int y = 0; y < board.Size; y++)
            {
                for (int x = 0; x < board.Size; x++)
                {
                    Tile tile = board.GetTile(x, y);
                    TileView view = tileViewsByValue[tile.Value];
                    view.Bind(tile);
                    view.MoveTo(GridToLocalPosition(x, y, board.Size), animate ? slideDuration : 0f);
                }
            }
        }

        private Vector2 GridToLocalPosition(int x, int y, int size)
        {
            float half = size * cellSize / 2f;
            float posX = -half + cellSize / 2f + x * cellSize;
            float posY = half - cellSize / 2f - y * cellSize;
            return new Vector2(posX, posY);
        }
    }
}
