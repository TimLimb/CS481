// Author: Timothy Hand
// Email: thand556@gmail.com
// Made with AI

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SlidingPuzzle.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ButtonPunch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private float pressedScale = 0.92f;
        [SerializeField] private float animationDuration = 0.08f;

        private RectTransform rectTransform;
        private Coroutine scaleRoutine;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AnimateTo(pressedScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            AnimateTo(1f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            AnimateTo(1f);
        }

        private void AnimateTo(float targetScale)
        {
            if (scaleRoutine != null)
            {
                StopCoroutine(scaleRoutine);
            }
            scaleRoutine = StartCoroutine(ScaleRoutine(targetScale));
        }

        private IEnumerator ScaleRoutine(float targetScale)
        {
            Vector3 start = rectTransform.localScale;
            Vector3 end = Vector3.one * targetScale;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                rectTransform.localScale = Vector3.Lerp(start, end, Mathf.Clamp01(elapsed / animationDuration));
                yield return null;
            }

            rectTransform.localScale = end;
            scaleRoutine = null;
        }
    }
}
