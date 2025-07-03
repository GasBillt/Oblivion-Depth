using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverChangeImageColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image targetImage; // сюда укажи картинку, цвет которой мен€етс€
    public Color hoverColor = Color.red;

    private Color originalColor;

    void Start()
    {
        if (targetImage == null)
        {
            // ѕопробуем найти родительский Image, если не задан €вно
            targetImage = GetComponentInParent<Image>();
        }

        if (targetImage != null)
        {
            originalColor = targetImage.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetImage != null)
        {
            targetImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage != null)
        {
            targetImage.color = originalColor;
        }
    }
}

