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
        Debug.Log("1");
        backupCoords = coords;
        StartCoroutine(DelayedFlash(delay, loc));
    }


    private IEnumerator DelayedFlash(float delay, string loc)
    {
        Debug.Log("2");
        yield return new WaitForSeconds(delay);
        FlashBang(loc);
    }

    private void FlashBang(string loc)
    {
        // Debug.Log("3");
        // flashbangImage.gameObject.SetActive(true);
        // flashbangImage.color = new Color(1f, 1f, 1f, 1f);
        StartCoroutine(FlashAnimation(loc));
    }

    private IEnumerator FlashAnimation(string loc)
    {
        Debug.Log("4");
        // Мгновенная телепортация при полной непрозрачности
        Teleporting(loc);

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
        if (Player == null)
        {
            Debug.LogError("Player reference is missing!");
            return;
        }

        // Случай с пустой локацией или "-"
        if (string.IsNullOrEmpty(loc) || loc == "-")
        {
            if (backupCoords == invalidCoords)
            {
                Debug.LogError("ErrorLog: Invalid backup coordinates");
                return;
            }
            Player.transform.position = backupCoords;
            return;
        }

        // Поиск индекса локации
        int index = System.Array.IndexOf(LevelManager.LocName, loc);
        if (index >= 0 && index < LevelManager.levelPoints.Length)
        {
            Player.transform.position = LevelManager.levelPoints[index];
        }
        else
        {
            // Резервные координаты при неудаче
            if (backupCoords == invalidCoords)
            {
                Debug.LogError($"ErrorLog: Location '{loc}' not found and invalid backup coordinates");
            }
            else
            {
                Player.transform.position = backupCoords;
            }
        }
    }
}