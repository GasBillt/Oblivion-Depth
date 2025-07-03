using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
public class PlayZoomFromButton : MonoBehaviour
{
    public float zoomSpeed = 2f;
    public float targetScale = 10f;

    private Transform background;
    private Transform menu;
    private bool isZooming = false;

    public string sceneName = "TestPlace";
    private bool zoomCompleted = false;

    void Start()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("PlayZoomFromButton: Не найден Canvas.");
            return;
        }

        background = canvas.transform.Find("Background");
        menu = canvas.transform.Find("GameObject");

        if (background == null)
            Debug.LogWarning("PlayZoomFromButton: Не найден объект 'Background' в Canvas.");
        if (menu == null)
            Debug.LogWarning("PlayZoomFromButton: Не найден объект 'GameObject' (меню) в Canvas.");

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(StartZoom);
        }
        else
        {
            Debug.LogError("PlayZoomFromButton должен быть на объекте с компонентом Button.");
        }
    }

    void Update()
    {
        if (!isZooming) return;

        bool done = true;

        if (background != null)
        {
            background.localScale = Vector3.Lerp(background.localScale, Vector3.one * targetScale, Time.deltaTime * zoomSpeed);
            if (Vector3.Distance(background.localScale, Vector3.one * targetScale) > 0.01f)
                done = false;
        }

        if (menu != null)
        {
            menu.localScale = Vector3.Lerp(menu.localScale, Vector3.one * targetScale, Time.deltaTime * zoomSpeed);
            if (Vector3.Distance(menu.localScale, Vector3.one * targetScale) > 0.01f)
                done = false;
        }

        if (done && !zoomCompleted)
        {
            zoomCompleted = true;
            isZooming = false;
            LoadTargetScene(); 
        }
    }

    private void LoadTargetScene()
    {
        if (SceneUtility.GetBuildIndexByScenePath(sceneName) >= 0)
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Сцена не найдена в Build Settings!");
        }
    }

    public void StartZoom()
    {
        isZooming = true;
        zoomCompleted = false; 
    }
}