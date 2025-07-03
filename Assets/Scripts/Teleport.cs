using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Teleport : MonoBehaviour
{
    public GameObject Player;
    public LevelManager LevelManager;
    public Image flashbangImage;
    private Vector3 backupCoords; // Хранит переданные координаты для телепортации
    private static readonly Vector3 invalidCoords = new Vector3(-1000, -1000, -1000);

    public void tp(float delay, string loc, Vector3 coords)
    {
        Debug.Log($"Teleport called to {loc}");
        backupCoords = coords;
        FindReferences(); // Перепроверяем ссылки перед запуском корутины
        StartCoroutine(DelayedFlash(delay, loc));
    }


    private void FindReferences()
    {
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("MainPlayer");
            if (Player == null) Debug.LogError("Player reference not found!");
        }

        if (LevelManager == null)
        {
            LevelManager = FindObjectOfType<LevelManager>();
            if (LevelManager == null) Debug.LogError("LevelManager reference not found!");
        }
    }

    private IEnumerator DelayedFlash(float delay, string loc)
    {
        Debug.Log("2");
        yield return new WaitForSeconds(delay);
        FlashBang(loc);
    }

    private void FlashBang(string loc)
    {
        Debug.Log("3");
        flashbangImage.gameObject.SetActive(true);
        flashbangImage.color = new Color(1f, 1f, 1f, 1f);
        StartCoroutine(FlashAnimation(loc));
    }

    private IEnumerator FlashAnimation(string loc)
    {
        FindReferences();
    
        if (LevelManager == null)
        {
            Debug.LogError("LevelManager is missing!");
            yield break;
        }
    
        // СНАЧАЛА телепортируем игрока
        Teleporting(loc);
        
        // ПОТОМ загружаем уровень
        LevelManager.LevelLoad(loc);
        LevelManager.currentLevel = loc;
    
        // Ожидаем конец кадра для применения изменений
        yield return new WaitForEndOfFrame();
    
        // Плавное исчезновение
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            flashbangImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
    
        flashbangImage.gameObject.SetActive(false);
    }

    private void Teleporting(string loc)
    {
        Debug.Log("5");
        FindReferences();

        if (Player == null)
        {
            Debug.LogError("Player reference is missing!");
            return;
        }

        // 1. Получаем Rigidbody и временно отключаем физику
        Rigidbody rb = Player.GetComponent<Rigidbody>();
        bool hadRigidbody = false;
        if (rb != null)
        {
            hadRigidbody = true;
            Vector3 savedVelocity = rb.linearVelocity;
            Vector3 savedAngularVelocity = rb.angularVelocity;

            // 2. Устанавливаем позицию через Rigidbody
            rb.position = backupCoords;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Debug.Log($"Teleported via Rigidbody to: {backupCoords}");

            // 3. Принудительное обновление физики
            Physics.SyncTransforms();
            return;
        }

        // 4. Если Rigidbody нет - используем обычную телепортацию
        Player.transform.position = backupCoords;
        Debug.Log($"Teleported via Transform to: {backupCoords}");

        // 5. Принудительное обновление трансформации
        Player.transform.SetPositionAndRotation(backupCoords, Player.transform.rotation);
    }
}