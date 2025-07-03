using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void OnQuitButtonClick()
    {
        // Работает в собранной версии игры
        Application.Quit();

        // Для теста в редакторе
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}