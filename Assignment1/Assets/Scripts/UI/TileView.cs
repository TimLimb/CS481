// Author: Timothy Hand
// Email: thand556@gmail.com
// Made with AI

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlidingPuzzle.Core;

namespace SlidingPuzzle.UI
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button button;
        [SerializeField] private Image cardImage;

        private RectTransform rectTransform;
        private Tile boundTile;
        private System.Action<Vector2Int> onSelected;
        private Coroutine moveRoutine;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            if (cardImage == null)
            {
                cardImage = GetComponent<Image>();
            }
        }

        public void Init(System.Action<Vector2Int> tileSelectedCallback)
        {
            onSelected = tileSelectedCallback;
            button.onClick.AddListener(OnClicked);
        }

        public void SetVisualSize(float size)
        {
            rectTransform.sizeDelta = new Vector2(size, size);
            label.fontSize = size * 0.42f;
        }

        public void Bind(Tile tile)
        {
            boundTile = tile;
            label.text = tile.IsBlank ? string.Empty : tile.Value.ToString();
            button.interactable = !tile.IsBlank;

            Color color = cardImage.color;
            color.a = tile.IsBlank ? 0f : 1f;
            cardImage.color = color;
        }

        public void MoveTo(Vector2 targetAnchoredPosition, float duration)
        {
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
                moveRoutine = null;
            }

            if (duration <= 0f)
            {
                rectTransform.anchoredPosition = targetAnchoredPosition;
                return;
            }

            moveRoutine = StartCoroutine(MoveRoutine(targetAnchoredPosition, duration));
        }

        private IEnumerator MoveRoutine(Vector2 target, float duration)
        {
            Vector2 start = rectTransform.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                rectTransform.anchoredPosition = Vector2.Lerp(start, target, t);
                yield return null;
            }

            rectTransform.anchoredPosition = target;
            moveRoutine = null;
        }

        private void OnClicked()
        {
            onSelected?.Invoke(boundTile.GridPosition);
        }
    }
}
