using System;
using System.Collections;
using UnityEngine;
using Util.UI.Controllers;

namespace Util.Helpers
{
    public static class UIHelper
    {
        // TODO: can probably remove this 
        public static IEnumerator FadeIn(CanvasGroup canvasGroup, Action after = null)
        {
            canvasGroup.alpha = 0f;

            while (canvasGroup.alpha < 1f)
            {
                canvasGroup.alpha += 0.05f;
                yield return null;
            }

            canvasGroup.alpha = 1f;
            if (after != null) after();
        }

        public static IEnumerator FadeOut(CanvasGroup canvasGroup, Action after = null)
        {
            canvasGroup.alpha = 1f;

            while (canvasGroup.alpha > 0f)
            {
                canvasGroup.alpha -= 0.05f;
                yield return null;
            }

            canvasGroup.alpha = 0f;
            if (after != null) after();
        }

        public static IEnumerator FadeInAndEnable(UIController uiController, CanvasGroup canvasGroup, float duration = 1f, Action after = null)
        {
            canvasGroup.alpha = 0f;
            uiController.gameObject.Enable();

            LeanTween.value(canvasGroup.gameObject, 0f, 1f, duration)
                .setOnUpdate((a) => canvasGroup.alpha = a)
                .setIgnoreTimeScale(true);

            yield return new WaitForSecondsRealtime(duration);

            after?.Invoke();
        }

        public static IEnumerator FadeOutAndDisable(UIController uiController, CanvasGroup canvasGroup, float duration = 1f, Action after = null)
        {
            canvasGroup.alpha = 1f;

            LeanTween.value(canvasGroup.gameObject, 1f, 0f, duration)
                .setOnUpdate((a) => canvasGroup.alpha = a)
                .setIgnoreTimeScale(true);

            yield return new WaitForSecondsRealtime(duration);

            uiController.gameObject.Disable();
            after?.Invoke();
        }
    }
}