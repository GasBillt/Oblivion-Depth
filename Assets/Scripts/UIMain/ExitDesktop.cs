using UnityEngine;

public class ExitToDesktop : MonoBehaviour
{
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Завершить игру в сборке
        Application.Quit();
#endif
    }
}
