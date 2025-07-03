using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class GifPlayerUI : MonoBehaviour
{
    public List<Sprite> frames;
    public Image targetImage;
    public float frameRate = 1f;
    public bool playOnStart = true;

    public Image pressAnyKeyImage;
    public float fadeInDuration = 1f;
    public UnityEvent onAnimationComplete;
    public UnityEvent onKeyPressed;

    public List<Sprite> secondGifFrames;
    public float secondGifFrameRate = 1f;
    public UnityEvent onSecondAnimationComplete;

    public List<Sprite> thirdGifFrames;
    public float thirdGifFrameRate = 1f;
    public UnityEvent onThirdAnimationComplete;

    public GameObject menuContainer;
    public float logoMoveSpeed = 500f;
    public float menuItemsMoveSpeed = 300f;
    public float logoLoopDistance = 10f;

    private bool FirstAnimationComplete = false;
    private bool WaitingForInput = false;
    private bool SecondAnimationComplete = false;
    private RectTransform logoRectTransform;
    private List<RectTransform> menuItems = new List<RectTransform>();
    private List<Vector2> menuItemsTargetPositions = new List<Vector2>();
    public bool isLogoMoving = false;
    public bool isMenuAppearing = false;
    public Vector2 logoLoopStartPosition;

    public float logoScaleDownFactor = 0.9f;
    public float logoScaleDuration = 0.5f;

    private Image Panel; 

    private void Start()
    {
        GameObject panelObj = GameObject.Find("Panel"); // Или ваше название объекта
        if (panelObj != null)
        {
            Panel = panelObj.GetComponent<Image>();
            if (Panel == null)
            {
                Debug.LogWarning("Panel найден, но у него нет компонента Image!");
            }
        }
        else
        {
            Debug.LogWarning("Panel не найден в сцене!");
        }

        if (pressAnyKeyImage != null)
        {
            pressAnyKeyImage.gameObject.SetActive(false);

            var canvasGroup = pressAnyKeyImage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = pressAnyKeyImage.gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
        }

        if (menuContainer != null)
        {
            menuContainer.SetActive(false);
        }

        logoRectTransform = targetImage.GetComponent<RectTransform>();

        if (playOnStart)
        {
            StartCoroutine(PlayGif());
        }
    }

    public void PlayAnimation()
    {
        StartCoroutine(PlayGif());
    }

    IEnumerator PlayGif()
    {
        for (int i = 0; i < frames.Count; i++)
        {
            targetImage.sprite = frames[i];
            yield return new WaitForSeconds(frameRate);
        }

        FirstAnimationComplete = true;

        if (pressAnyKeyImage != null)
        {
            pressAnyKeyImage.gameObject.SetActive(true);
            CanvasGroup canvasGroup = pressAnyKeyImage.GetComponent<CanvasGroup>();
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        WaitingForInput = true;
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        WaitingForInput = false;

        if (secondGifFrames != null && secondGifFrames.Count > 0)
        {
            yield return StartCoroutine(PlaySecondGif());
        }

        if (SecondAnimationComplete == true && thirdGifFrames != null && thirdGifFrames.Count > 0)
        {
            yield return StartCoroutine(PlayThirdGif());
        }

        yield return StartCoroutine(StartFinalAnimation());
    }

    IEnumerator PlaySecondGif()
    {
        List<FrameSegment> segments = new List<FrameSegment>
        {
            new FrameSegment(0, 6),
            new FrameSegment(7, 13),
            new FrameSegment(14, 20),
            new FrameSegment(21, secondGifFrames.Count - 1)
        };

        for (int segIndex = 0; segIndex < segments.Count; segIndex++)
        {
            FrameSegment segment = segments[segIndex];
            for (int i = segment.Start; i <= segment.End; i++)
            {
                if (i < secondGifFrames.Count)
                {
                    targetImage.sprite = secondGifFrames[i];
                    yield return new WaitForSeconds(secondGifFrameRate);
                }
            }

            if (segIndex < segments.Count - 1)
            {
                WaitingForInput = true;
                while (!Input.anyKeyDown)
                {
                    yield return null;
                }
                WaitingForInput = false;
            }
        }

        SecondAnimationComplete = true;
    }

    IEnumerator PlayThirdGif()
    {
        for (int i = 0; i < thirdGifFrames.Count; i++)
        {
            targetImage.sprite = thirdGifFrames[i];
            yield return new WaitForSeconds(thirdGifFrameRate);
        }

        if (Panel != null)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Color startColor = Panel.color;
            Color targetColor = startColor;
            targetColor.a = 0f;

            while (elapsed < duration)
            {
                Panel.color = Color.Lerp(startColor, targetColor, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Panel.color = targetColor;
        }
    }

    IEnumerator StartFinalAnimation()
    {
        if (pressAnyKeyImage != null)
        {
            pressAnyKeyImage.gameObject.SetActive(false);
        }

        if (menuContainer != null)
        {
            menuContainer.SetActive(true);
            menuItems.Clear();
            menuItemsTargetPositions.Clear();

            foreach (Transform child in menuContainer.transform)
            {
                var rectTransform = child.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Frame fade only, no movement
                    if (child.name == "Frame")
                    {
                        CanvasGroup group = child.GetComponent<CanvasGroup>();
                        if (group == null)
                            group = child.gameObject.AddComponent<CanvasGroup>();
                        group.alpha = 0f;
                        StartCoroutine(FadeInCanvasGroup(group, 0.5f));
                        continue;
                    }

                    // Regular menu items move in
                    menuItems.Add(rectTransform);
                    menuItemsTargetPositions.Add(rectTransform.anchoredPosition);
                    Vector2 startPos = rectTransform.anchoredPosition;
                    startPos.x = -rectTransform.rect.width - 100f;
                    rectTransform.anchoredPosition = startPos;
                }
            }
        }

        isLogoMoving = true;
        logoLoopStartPosition = logoRectTransform.anchoredPosition;
        StartCoroutine(MoveAndScaleLogo());

        isMenuAppearing = true;
        StartCoroutine(MoveMenuItemsToTargetPositions());

        yield return null;
    }

    IEnumerator MoveAndScaleLogo()
    {
        float startX = logoRectTransform.anchoredPosition.x;
        float targetX = startX + logoLoopDistance;
        float distance = Mathf.Abs(targetX - startX);
        float moveDuration = distance / logoMoveSpeed;

        Vector3 startScale = logoRectTransform.localScale;
        Vector3 targetScale = startScale * logoScaleDownFactor;

        float elapsed = 0f;
        while (elapsed < Mathf.Max(moveDuration, logoScaleDuration))
        {
            if (elapsed < moveDuration)
            {
                float newX = Mathf.Lerp(startX, targetX, elapsed / moveDuration);
                logoRectTransform.anchoredPosition = new Vector2(newX, logoRectTransform.anchoredPosition.y);
            }

            if (elapsed < logoScaleDuration)
            {
                float scaleProgress = elapsed / logoScaleDuration;
                logoRectTransform.localScale = Vector3.Lerp(startScale, targetScale, scaleProgress);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        logoRectTransform.anchoredPosition = new Vector2(targetX, logoRectTransform.anchoredPosition.y);
        logoRectTransform.localScale = targetScale;

        StartCoroutine(LogoLoopAnimation());
    }

    IEnumerator LogoLoopAnimation()
    {
        while (true)
        {
            for (int i = 0; i < thirdGifFrames.Count; i++)
            {
                targetImage.sprite = thirdGifFrames[i];
                yield return new WaitForSeconds(thirdGifFrameRate);
            }
        }
    }

    IEnumerator MoveMenuItemsToTargetPositions()
    {
        float longestDuration = 0f;
        for (int i = 0; i < menuItems.Count; i++)
        {
            float distance = Mathf.Abs(menuItems[i].anchoredPosition.x - menuItemsTargetPositions[i].x);
            float duration = distance / menuItemsMoveSpeed;
            if (duration > longestDuration) longestDuration = duration;

            StartCoroutine(MoveMenuItem(i, menuItemsMoveSpeed));
        }

        yield return new WaitForSeconds(longestDuration);
        isMenuAppearing = false;
    }

    IEnumerator MoveMenuItem(int index, float speed)
    {
        RectTransform item = menuItems[index];
        Vector2 targetPos = menuItemsTargetPositions[index];
        Vector2 startPos = item.anchoredPosition;

        float distance = Mathf.Abs(targetPos.x - startPos.x);
        float duration = distance / speed;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float newX = Mathf.Lerp(startPos.x, targetPos.x, elapsed / duration);
            item.anchoredPosition = new Vector2(newX, targetPos.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        item.anchoredPosition = targetPos;
    }

    IEnumerator FadeInCanvasGroup(CanvasGroup group, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            group.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        group.alpha = 1f;
    }

    private class FrameSegment
    {
        public int Start { get; }
        public int End { get; }

        public FrameSegment(int start, int end)
        {
            Start = start;
            End = end;
        }
    }

    public bool IsWaitingForInput()
    {
        return WaitingForInput;
    }

    public bool IsFirstAnimationComplete()
    {
        return FirstAnimationComplete;
    }
}
