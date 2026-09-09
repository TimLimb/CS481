// Author: Timothy Hand
// Email: thand556@gmail.com
// Made with AI

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SlidingPuzzle.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject titlePanel;
        [SerializeField] private GameObject controlsPanel;
        [SerializeField] private Button size3Button;
        [SerializeField] private Button size4Button;
        [SerializeField] private Button size5Button;
        [SerializeField] private Button hardModeButton;
        [SerializeField] private Button shuffleButton;
        [SerializeField] private Button solveButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private GameObject boardArea;
        [SerializeField] private TMP_Text winMessageText;
        [SerializeField] private TMP_Text moveCounterText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private CanvasGroup winCanvasGroup;
        [SerializeField] private RectTransform winCardTransform;
        [SerializeField] private CanvasGroup titleCanvasGroup;
        [SerializeField] private float popInDuration = 0.25f;

        private Coroutine winAnimRoutine;
        private Coroutine titleAnimRoutine;

        public Button Size3Button => size3Button;
        public Button Size4Button => size4Button;
        public Button Size5Button => size5Button;
        public Button HardModeButton => hardModeButton;
        public Button ShuffleButton => shuffleButton;
        public Button SolveButton => solveButton;
        public Button RestartButton => restartButton;
        public Button ContinueButton => continueButton;
        public Button HomeButton => homeButton;

        public void SetBoardVisible(bool visible)
        {
            if (boardArea != null)
            {
                boardArea.SetActive(visible);
            }
        }

        public void ShowWinPanel(string message, bool canContinue)
        {
            if (winMessageText != null)
            {
                winMessageText.text = message;
            }
            winPanel.SetActive(true);

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(canContinue);
            }
            // Re-center Restart/Home as a pair when Continue is hidden.
            SetButtonX(restartButton, canContinue ? 0f : -72f);
            SetButtonX(homeButton, canContinue ? 145f : 72f);

            if (winAnimRoutine != null)
            {
                StopCoroutine(winAnimRoutine);
            }
            winAnimRoutine = StartCoroutine(PlayPopIn(winCanvasGroup, winCardTransform));
        }

        private void SetButtonX(Button button, float x)
        {
            if (button == null)
            {
                return;
            }

            var rt = (RectTransform)button.transform;
            Vector2 pos = rt.anchoredPosition;
            pos.x = x;
            rt.anchoredPosition = pos;
        }

        public void HideWinPanel()
        {
            winPanel.SetActive(false);
        }

        public void UpdateMoveCounter(int moves, int? parMoves)
        {
            if (moveCounterText == null)
            {
                return;
            }

            moveCounterText.text = parMoves.HasValue
                ? $"Moves: {moves}  (Par: {parMoves.Value})"
                : $"Moves: {moves}";
        }

        public void UpdateScore(int score)
        {
            if (scoreText == null)
            {
                return;
            }

            scoreText.text = $"Score: {score}";
        }

        public void ShowTitlePanel()
        {
            titlePanel.SetActive(true);

            if (titleAnimRoutine != null)
            {
                StopCoroutine(titleAnimRoutine);
            }
            titleAnimRoutine = StartCoroutine(PlayPopIn(titleCanvasGroup, null));
        }

        public void HideTitlePanel()
        {
            titlePanel.SetActive(false);
        }

        private IEnumerator PlayPopIn(CanvasGroup canvasGroup, RectTransform card)
        {
            float elapsed = 0f;
            Vector3 startScale = card != null ? Vector3.one * 0.85f : Vector3.one;
            if (card != null)
            {
                card.localScale = startScale;
            }
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            while (elapsed < popInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / popInDuration));
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = t;
                }
                if (card != null)
                {
                    card.localScale = Vector3.Lerp(startScale, Vector3.one, t);
                }
                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
            if (card != null)
            {
                card.localScale = Vector3.one;
            }
        }

        public void ShowControlsPanel()
        {
            controlsPanel.SetActive(true);
        }

        public void HideControlsPanel()
        {
            controlsPanel.SetActive(false);
        }
    }
}
