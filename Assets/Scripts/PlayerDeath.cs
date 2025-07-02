using UnityEngine;


public class PlayerDeath : MonoBehaviour
{
    [Header("Death Settings")]
    public string sceneToLoad = ""; // Имя сцены для загрузки
    public DeathManager deathManager;
    private bool isTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        // Проверяем тэг входящего объекта
        if (other.CompareTag("MainPlayer") && !isTriggered)
        {
            isTriggered = true;
            deathManager.Death(sceneToLoad);
        }
    }
}