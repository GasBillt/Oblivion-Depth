using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void OnQuitButtonClick()
    {
        // �������� � ��������� ������ ����
        Application.Quit();

        // ��� ����� � ���������
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}