using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Teleport : MonoBehaviour
{
//    public GameObject Player;
//    public Image flashbangImage;

//    public void tp(float delay, string loc)
//    {
//        StartCoroutine(TeleportWithEffects(delay, loc));
//    }

//    private IEnumerator TeleportWithEffects(float delay, string loc)
//    {
//        yield return new WaitForSeconds(delay);
//        yield return StartCoroutine(FlashbangEffect());

//        if (!string.IsNullOrEmpty(loc) && loc != "-")
//        //{
//            // Проверяем существование LevelManager
//            //if (LevelManager.Instance != null)
//            //{
//            //    // Парсим координаты из строки формата "i,j"
//            //    string[] indices = loc.Split(',');
//            //    if (indices.Length != 2)
//            //    {
//            //        Debug.LogError("Invalid location format! Use 'i,j'");
//            //        yield break;
//            //    }

//            //    int i, j;
//            //    if (!int.TryParse(indices[0], out i) || !int.TryParse(indices[1], out j))
//            //    {
//            //        Debug.LogError("Invalid indices! Must be integers.");
//            //        yield break;
//            //    }

//            //    // Получаем координаты из LevelManager
//            //    Vector3 targetPosition = LevelManager.Instance.GetCoords(i, j);

//            //    // Телепортируем игрока
//            //    Player.transform.position = targetPosition;
//            //}
//           //else
//           // {
//           //     Debug.LogError("LevelManager instance not found!");
//           // }
//        }
//    }

//    //private IEnumerator FlashbangEffect()
//    //{
//    //    if (flashbangImage == null)
//    //    {
//    //        Debug.LogError("Flashbang Image reference is missing!");
//    //        yield break;
//    //    }

//    //    flashbangImage.gameObject.SetActive(true);
//    //    Color color = Color.white;
//    //    color.a = 0f;
//    //    flashbangImage.color = color;

//    //    float duration = 0.1f;
//    //    float elapsed = 0f;
//    //    while (elapsed < duration)
//    //    {
//    //        elapsed += Time.deltaTime;
//    //        color.a = Mathf.Lerp(0f, 1f, elapsed / duration);
//    //        flashbangImage.color = color;
//    //        yield return null;
//    //    }

//    //    yield return new WaitForSeconds(0.5f);

//    //    elapsed = 0f;
//    //    duration = 0.3f;
//    //    while (elapsed < duration)
//    //    {
//    //        elapsed += Time.deltaTime;
//    //        color.a = Mathf.Lerp(1f, 0f, elapsed / duration);
//    //        flashbangImage.color = color;
//    //        yield return null;
//    //    }

//    //    flashbangImage.gameObject.SetActive(false);
//    //}
}