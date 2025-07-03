using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverChangeImageColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image targetImage; // ���� ����� ��������, ���� ������� ��������
    public Color hoverColor = Color.red;

    private Color originalColor;

    void Start()
    {
        if (targetImage == null)
        {
            // ��������� ����� ������������ Image, ���� �� ����� ����
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

