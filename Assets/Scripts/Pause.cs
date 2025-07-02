using UnityEngine;

public class GamePauseController : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Tab))
        {
            Pause();
        }
    }

    void Pause()
    {
        if (Time.timeScale > 0)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;

        Debug.Log("Игра поставлена на паузу");
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Debug.Log("Игра продолжена");
    }
}