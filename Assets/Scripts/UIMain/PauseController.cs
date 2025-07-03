using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [SerializeField] public GameObject PauseUI;
    private bool isPaused = false;
    private CursorLockMode previousLockState;
    private bool previousCursorVisible;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Сохраняем текущее состояние курсора перед паузой
            previousLockState = Cursor.lockState;
            previousCursorVisible = Cursor.visible;

            // Останавливаем игру
            Time.timeScale = 0f;
            PauseUI.SetActive(true);

            // Разблокируем курсор
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Возобновляем игру
            Time.timeScale = 1f;
            PauseUI.SetActive(false);

            // Восстанавливаем предыдущее состояние курсора
            Cursor.lockState = previousLockState;
            Cursor.visible = previousCursorVisible;
        }
    }

    public void OnContinueButton()
    {
        TogglePause();
    }
}