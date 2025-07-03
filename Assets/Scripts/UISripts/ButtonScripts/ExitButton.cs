using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuitButton : MonoBehaviour, IPointerClickHandler
{
    public float exitDelay = 0.5f; 
    public bool useFadeEffect = true; 
    public float fadeDuration = 0.3f; 

    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(ExitGame());
    }

    private System.Collections.IEnumerator ExitGame()
    {
        if (useFadeEffect)
        {
            Image fadeImage = CreateFadeImage();
            float elapsed = 0f;
            Color startColor = new Color(0f, 0f, 0f, 0f);
            Color targetColor = Color.black;

            while (elapsed < fadeDuration)
            {
                fadeImage.color = Color.Lerp(startColor, targetColor, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            fadeImage.color = targetColor;
        }
        yield return new WaitForSeconds(exitDelay);
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private Image CreateFadeImage()
    {
        GameObject fadeObject = new GameObject("ExitFade");
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("ExitCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
        }
        fadeObject.transform.SetParent(canvas.transform, false);
        Image fadeImage = fadeObject.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.rectTransform.anchorMin = Vector2.zero;
        fadeImage.rectTransform.anchorMax = Vector2.one;
        fadeImage.rectTransform.sizeDelta = Vector2.zero;

        return fadeImage;
    }
}